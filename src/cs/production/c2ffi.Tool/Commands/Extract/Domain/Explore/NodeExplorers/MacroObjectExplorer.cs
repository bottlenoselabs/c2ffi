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

    public MacroObjectExplorer(
        ILogger<MacroObjectExplorer> logger,
        IFileSystem fileSystem,
        ClangTranslationUnitParser clangTranslationUnitParser)
        : base(logger, false)
    {
        _fileSystem = fileSystem;
    }

    protected override ExploreKindCursors ExpectedCursors =>
        ExploreKindCursors.Is(clang.CXCursorKind.CXCursor_MacroDefinition);

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override bool IsAllowed(ExploreContext context, ExploreNodeInfo info)
    {
        var ignoredMacroObjects = context.ParseContext.ExtractOptions.IgnoredMacroObjects;
        return ignoredMacroObjects.IsEmpty || !ignoredMacroObjects.Contains(info.Name);
    }

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return MacroObject(context, info);
    }

    private CMacroObject MacroObject(ExploreContext context, ExploreNodeInfo info)
    {
        var macroObjectCandidate = MacroObjectCandidate.Parse(info.Cursor, context.ParseContext.SystemIncludeDirectories);
        if (macroObjectCandidate == null)
        {
            throw new InvalidOperationException($"Failed to parse macro object '{info.Name}'.");
        }

        var filePath = WriteMacroObjectsFile(macroObjectCandidate);
        var macroObject = GetMacroObjectFromParsingFile(filePath, context.ParseContext);
        if (macroObject == null)
        {
            throw new InvalidOperationException($"Failed to parse macro object '{info.Name}'.");
        }

        return macroObject;
    }

    private string WriteMacroObjectsFile(MacroObjectCandidate macroObjectCandidate)
    {
        var tempFilePath = _fileSystem.Path.GetTempFileName();
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

    private CMacroObject? GetMacroObjectFromParsingFile(
        string filePath, ParseContext originalParseContext)
    {
        if (!ClangExtensions.TryParseTranslationUnit(
                filePath,
                originalParseContext.Arguments,
                out var translationUnit,
                false))
        {
            return null;
        }

        var translationUnitCursor = clang.clang_getTranslationUnitCursor(translationUnit);
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
                @"Failed to parse C++ file to determine types of macro objects. Please ensure your libclang version is up-to-date.");
        }

        var compoundStatement = functionCursor.GetDescendents(static (cursor, _) =>
                cursor.kind == clang.CXCursorKind.CXCursor_CompoundStmt)
            .FirstOrDefault();
        var declarationStatement =
            compoundStatement.GetDescendents(static (cursor, _) =>
                cursor.kind == clang.CXCursorKind.CXCursor_DeclStmt).FirstOrDefault();
        var readerLineNumber = 0;

        var variable = declarationStatement.GetDescendents(static (cursor, _) =>
                cursor.kind == clang.CXCursorKind.CXCursor_VarDecl)
            .FirstOrDefault();
        var variableName = variable.Spelling();
        var macroName =
            variableName.Replace("variable_", string.Empty, StringComparison.InvariantCultureIgnoreCase);
        var cursor = variable.GetDescendents().FirstOrDefault();

        var type = clang.clang_getCursorType(cursor);

        var value = EvaluateMacroValue(cursor, type);
        if (value == null)
        {
            return null;
        }

        using var streamReader = new StreamReader(filePath);
        var location = MacroLocation(cursor, originalParseContext.SystemIncludeDirectories, streamReader, ref readerLineNumber);

        var nodeKind = MacroTypeNodeKind(type);
        var typeName = type.Spelling();
        var sizeOf = (int)clang.clang_Type_getSizeOf(type);
        var typeInfo = new CTypeInfo
        {
            Name = typeName,
            NodeKind = nodeKind,
            SizeOf = sizeOf
        };

        var macroObject = new CMacroObject
        {
            Name = macroName,
            Value = value,
            TypeInfo = typeInfo,
            Location = location
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

    private CLocation MacroLocation(
        clang.CXCursor cursor,
        ImmutableArray<string> systemIncludeDirectories,
        StreamReader reader,
        ref int readerLineNumber)
    {
        var location = cursor.Location(systemIncludeDirectories);
        var locationCommentLineNumber = location.LineNumber - 1;

        if (readerLineNumber > locationCommentLineNumber)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            readerLineNumber = 0;
        }

        var line = string.Empty;
        while (readerLineNumber != locationCommentLineNumber)
        {
            line = reader.ReadLine() ?? string.Empty;
            readerLineNumber++;
        }

        var locationString = line.Trim().TrimStart('/').Trim();
        var lineIndex = locationString.IndexOf(':', StringComparison.InvariantCulture);
        var columnIndex = locationString.IndexOf(':', lineIndex + 1);
        var filePathIndex = locationString.IndexOf('(', StringComparison.InvariantCulture);

        int columnIndexEnd;
        if (filePathIndex == -1)
        {
            columnIndexEnd = locationString.Length;
        }
        else
        {
            columnIndexEnd = filePathIndex - 1;
        }

        var lineString = locationString[(lineIndex + 1).. columnIndex];
        var columnString = locationString[(columnIndex + 1).. columnIndexEnd];
        var fileNameString = locationString[..lineIndex];

        var filePathString = filePathIndex == -1 ? fileNameString : locationString[(filePathIndex + 1)..^1];
        var lineNumber = int.Parse(lineString, CultureInfo.InvariantCulture);
        var lineColumn = int.Parse(columnString, CultureInfo.InvariantCulture);

        var actualLocation = new CLocation
        {
            FileName = fileNameString,
            FilePath = filePathString,
            LineNumber = lineNumber,
            LineColumn = lineColumn
        };
        return actualLocation;
    }

    private static CNodeKind MacroTypeNodeKind(clang.CXType type)
    {
        if (type.IsPrimitive())
        {
            return CNodeKind.Primitive;
        }

        return type.kind switch
        {
            clang.CXTypeKind.CXType_Typedef => CNodeKind.TypeAlias,
            clang.CXTypeKind.CXType_Enum => CNodeKind.Enum,
            clang.CXTypeKind.CXType_Pointer => CNodeKind.Pointer,
            clang.CXTypeKind.CXType_ConstantArray => CNodeKind.Array,
            _ => CNodeKind.Unknown
        };
    }

    private sealed class MacroObjectCandidate
    {
        public string Name { get; init; } = string.Empty;

        public CLocation? Location { get; init; }

        public ImmutableArray<string> Tokens { get; init; } = ImmutableArray<string>.Empty;

        public static MacroObjectCandidate? Parse(
            clang.CXCursor clangCursor,
            ImmutableArray<string> systemIncludeDirectories)
        {
            var name = clangCursor.Spelling();
            var location = clangCursor.Location(systemIncludeDirectories);

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
