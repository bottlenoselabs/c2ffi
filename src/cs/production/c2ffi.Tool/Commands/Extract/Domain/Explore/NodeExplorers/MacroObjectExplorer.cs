// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using bottlenoselabs;
using bottlenoselabs.Common.Tools;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using c2ffi.Tool.Commands.Extract.Domain.Parse;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class MacroObjectExplorer : NodeExplorer<COpaqueType>
{
    private readonly IFileSystem _fileSystem;
    private readonly ClangTranslationUnitParser _clangTranslationUnitParser;

    public MacroObjectExplorer(
        ILogger<MacroObjectExplorer> logger,
        IFileSystem fileSystem,
        ClangTranslationUnitParser clangTranslationUnitParser)
        : base(logger, false)
    {
        _fileSystem = fileSystem;
        _clangTranslationUnitParser = clangTranslationUnitParser;
    }

    protected override ExploreKindCursors ExpectedCursors =>
        ExploreKindCursors.Is(clang.CXCursorKind.CXCursor_MacroDefinition);

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override bool IsAllowed(ExploreContext context, ExploreNodeInfo info)
    {
        var ignoredMacroObjectRegexes = context.ParseContext.ExtractOptions.IgnoredMacroObjectsRegexes;
        foreach (var regex in ignoredMacroObjectRegexes)
        {
            if (regex.IsMatch(info.Name))
            {
                return false;
            }
        }

        return true;
    }

    protected override CNode? GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return MacroObject(context, info);
    }

    private CMacroObject? MacroObject(ExploreContext context, ExploreNodeInfo info)
    {
        var macroObjectCandidate = MacroObjectCandidate.Parse(context.ParseContext, info.Cursor);
        if (macroObjectCandidate == null)
        {
            return null;
        }

        var filePath = WriteMacroObjectsFile(macroObjectCandidate);
        CMacroObject? macroObject;
        try
        {
            macroObject = GetMacroObjectFromParsingFile(filePath, context, info);
        }
        finally
        {
            if (_fileSystem.File.Exists(filePath))
            {
                _fileSystem.File.Delete(filePath);
            }
        }

        return macroObject;
    }

    private string WriteMacroObjectsFile(MacroObjectCandidate macroObjectCandidate)
    {
        var tempFilePath = _fileSystem.Path.ChangeExtension(_fileSystem.Path.GetTempFileName(), ".txt");
        using var fileStream = _fileSystem.File.OpenWrite(tempFilePath);
        using var writer = new StreamWriter(fileStream);

        var includeHeaderFilePath = macroObjectCandidate.Location!.Value.FullFilePath;

        writer.Write("#include \"");
        writer.Write(includeHeaderFilePath);
        writer.WriteLine("\"");

        const string codeStart = @"
int main(void)
{";
        writer.WriteLine(codeStart);

        writer.WriteLine("\t// " + macroObjectCandidate.Location);
        writer.Write("\tauto variable_");
        writer.Write(macroObjectCandidate.Name);
        writer.Write(" = ");
        foreach (var token in macroObjectCandidate.Tokens)
        {
            writer.Write(token);
        }

        writer.WriteLine(";");

        const string codeEnd = @"
}";
        writer.WriteLine(codeEnd);
        writer.Flush();
        writer.Close();

        return tempFilePath;
    }

    private CMacroObject? GetMacroObjectFromParsingFile(string filePath, ExploreContext context, ExploreNodeInfo info)
    {
        using var parseContext = _clangTranslationUnitParser.ParseTranslationUnit(
            filePath,
            context.ParseContext.ExtractOptions,
            isCPlusPlus: true,
            ignoreWarnings: true,
            logClangDiagnostics: false,
            keepGoing: true,
            skipFunctionBodies: false);

        var translationUnitCursor = clang.clang_getTranslationUnitCursor(parseContext.TranslationUnit);
        var functionCursor = translationUnitCursor
            .GetDescendents(static (cursor, _) =>
            {
                var sourceLocation = clang.clang_getCursorLocation(cursor);
                var isFromMainFile = clang.clang_Location_isFromMainFile(sourceLocation) > 0;
                if (!isFromMainFile)
                {
                    return false;
                }

                return cursor.kind == clang.CXCursorKind.CXCursor_FunctionDecl;
            }).FirstOrDefault();
        if (functionCursor.kind == 0)
        {
            throw new ToolException(
                "Failed to parse C++ file to determine types of macro objects. Please ensure your libclang version is up-to-date.");
        }

        var compoundStatement = functionCursor.GetDescendents(static (cursor, _) =>
                cursor.kind == clang.CXCursorKind.CXCursor_CompoundStmt)
            .FirstOrDefault();
        var declarationStatement =
            compoundStatement.GetDescendents(static (cursor, _) =>
                cursor.kind == clang.CXCursorKind.CXCursor_DeclStmt).FirstOrDefault();

        var variable = declarationStatement.GetDescendents(static (cursor, _) =>
                cursor.kind == clang.CXCursorKind.CXCursor_VarDecl)
            .FirstOrDefault();
        var variableName = variable.Spelling();
        var macroName =
            variableName.Replace("variable_", string.Empty, StringComparison.InvariantCultureIgnoreCase);
        var variableDescendents = variable.GetDescendents();
        if (variableDescendents.IsDefaultOrEmpty)
        {
            return null;
        }

        var clangCursor = variableDescendents.FirstOrDefault();
        var clangType = clang.clang_getCursorType(clangCursor);
        if (clangType.kind == clang.CXTypeKind.CXType_Invalid)
        {
            return null;
        }

        var value = EvaluateMacroValue(clangCursor, clangType);
        if (value == null)
        {
            return null;
        }

        var typeInfo = context.VisitType(clangType, info);
        var macroObject = new CMacroObject
        {
            Name = macroName,
            Value = value,
            TypeInfo = typeInfo,
            Location = info.Location
        };

        return macroObject;
    }

    private static string? EvaluateMacroValue(clang.CXCursor cursor, clang.CXType type)
    {
        var evaluateResult = clang.clang_Cursor_Evaluate(cursor);
        var kind = clang.clang_EvalResult_getKind(evaluateResult);
        string value;

        switch (kind)
        {
            case clang.CXEvalResultKind.CXEval_UnExposed:
                return null;
            case clang.CXEvalResultKind.CXEval_Int:
            {
                var canonicalType = clang.clang_getCanonicalType(type);
                if (canonicalType.IsSignedPrimitive())
                {
                    var integerValueSigned = clang.clang_EvalResult_getAsInt(evaluateResult);
                    value = integerValueSigned.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    var integerValueUnsigned = clang.clang_EvalResult_getAsUnsigned(evaluateResult);
                    value = integerValueUnsigned.ToString(CultureInfo.InvariantCulture);
                }

                break;
            }

            case clang.CXEvalResultKind.CXEval_StrLiteral or clang.CXEvalResultKind.CXEval_CFStr:
            {
                var stringValueC = clang.clang_EvalResult_getAsStr(evaluateResult);
                var stringValue = Marshal.PtrToStringAnsi(stringValueC)!;
                value = "\"" + stringValue + "\"";
                break;
            }

            case clang.CXEvalResultKind.CXEval_Float:
            {
                var floatValue = clang.clang_EvalResult_getAsDouble(evaluateResult);
                value = floatValue.ToString(CultureInfo.InvariantCulture);
                break;
            }

            default:
                throw new NotImplementedException();
        }

        clang.clang_EvalResult_dispose(evaluateResult);
        return value;
    }

    private sealed class MacroObjectCandidate
    {
        public string Name { get; init; } = string.Empty;

        public CLocation? Location { get; init; }

        public ImmutableArray<string> Tokens { get; init; } = ImmutableArray<string>.Empty;

        public static MacroObjectCandidate? Parse(
            ParseContext parseContext,
            clang.CXCursor clangCursor)
        {
            var name = clangCursor.Spelling();
            var location = parseContext.Location(clangCursor);

            // clang doesn't have a thing where we can easily get a value of a macro
            // we need to:
            //  1. get the text range of the cursor
            //  2. get the tokens over said text range
            //  3. go through the tokens to parse the value
            // this means we get to do token parsing ourselves
            // NOTE: The first token will always be the name of the macro
            var translationUnit = clang.clang_Cursor_getTranslationUnit(clangCursor);
            string[] tokens;
            unsafe
            {
                var range = clang.clang_getCursorExtent(clangCursor);
                var tokensC = (clang.CXToken*)0;
                uint tokensCount = 0;

                clang.clang_tokenize(translationUnit, range, &tokensC, &tokensCount);

                var isFlag = tokensCount is 0 or 1;
                if (isFlag)
                {
                    clang.clang_disposeTokens(translationUnit, tokensC, tokensCount);
                    return null;
                }

                tokens = new string[tokensCount - 1];
                for (var i = 1; i < (int)tokensCount; i++)
                {
                    var tokenString = clang.clang_getTokenSpelling(translationUnit, tokensC[i]).String();

                    // CLANG BUG?: https://github.com/FNA-XNA/FAudio/blob/b84599a5e6d7811b02329709a166a337de158c5e/include/FAPOBase.h#L90
                    if (tokenString.StartsWith('\\'))
                    {
                        tokenString = tokenString.TrimStart('\\');
                    }

                    if (tokenString.StartsWith("__", StringComparison.InvariantCulture) &&
                        tokenString.EndsWith("__", StringComparison.InvariantCulture))
                    {
                        clang.clang_disposeTokens(translationUnit, tokensC, tokensCount);
                        return null;
                    }

                    tokens[i - 1] = tokenString.Trim();
                }

                clang.clang_disposeTokens(translationUnit, tokensC, tokensCount);
            }

            var tokensImmutable = tokens.ToImmutableArray();
            if (tokensImmutable.IsDefaultOrEmpty)
            {
                return null;
            }

            var result = new MacroObjectCandidate
            {
                Name = name,
                Tokens = tokensImmutable,
                Location = location
            };

            return result;
        }
    }
}
