// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs;
using c2ffi.Clang;
using c2ffi.Data;
using c2ffi.Extract.Parse;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ClangTranslationUnitParser = c2ffi.Extract.Parse.ClangTranslationUnitParser;

namespace c2ffi.Extract.Explore;

[UsedImplicitly]
public sealed partial class Explorer(
    ILogger<Explorer> logger,
    IServiceProvider services,
    ClangTranslationUnitParser clangTranslationUnitParser)
{
    private readonly List<ParseContext> _parseContexts = [];

    private readonly HashSet<string> _visitedIncludeFilePaths = [];

    public CFfiTargetPlatform ExtractFfi(
        string filePath,
        InputSanitizedTargetPlatform input)
    {
        using var context = CreateExploreContext(filePath, input);

        CFfiTargetPlatform result;
        try
        {
            VisitTranslationUnit(context, context.ParseContext);
            result = context.GetFfi();
            LogFound(result);
        }
        catch (Exception e)
        {
            LogFailure(e);
            throw;
        }
        finally
        {
            _visitedIncludeFilePaths.Clear();
            // TODO: Refactor.
            foreach (var parseContext in _parseContexts)
            {
                parseContext.Dispose();
            }
        }

        LogSuccess();
        return result;
    }

    private void VisitTranslationUnit(ExploreContext exploreContext, ParseContext parseContext)
    {
        LogVisitingTranslationUnit(parseContext.FilePath);

        VisitIncludes(exploreContext, parseContext);
        VisitFunctions(exploreContext, parseContext);
        VisitVariables(exploreContext, parseContext);
        VisitMacroObjects(exploreContext, parseContext);
        VisitExplicitIncludedNames(exploreContext, parseContext);

        LogVisitedTranslationUnit(parseContext.FilePath);
    }

    private void VisitFunctions(ExploreContext exploreContext, ParseContext parseContext)
    {
        var functionCursors = parseContext.GetExternalFunctions(parseContext);
        foreach (var cursor in functionCursors)
        {
            VisitFunction(exploreContext, cursor);
        }
    }

    private void VisitFunction(ExploreContext context, clang.CXCursor clangCursor)
    {
        var info = context.CreateNodeInfoFunction(clangCursor);
        context.TryEnqueueNode(info);
    }

    private void VisitVariables(ExploreContext context, ParseContext parseContext)
    {
        var variableCursors = parseContext.GetExternalVariables();
        foreach (var cursor in variableCursors)
        {
            VisitVariable(context, cursor);
        }
    }

    private void VisitVariable(ExploreContext context, clang.CXCursor clangCursor)
    {
        var info = context.CreateNodeInfoVariable(clangCursor);
        context.TryEnqueueNode(info);
    }

    private void VisitMacroObjects(ExploreContext context, ParseContext parseContext)
    {
        var macroObjectCursors = parseContext.GetMacroObjects();
        foreach (var cursor in macroObjectCursors)
        {
            VisitMacroObject(context, cursor);
        }
    }

    private void VisitMacroObject(ExploreContext context, clang.CXCursor clangCursor)
    {
        var info = context.CreateNodeInfoMacroObject(clangCursor);
        context.TryEnqueueNode(info);
    }

    private void VisitExplicitIncludedNames(ExploreContext exploreContext, ParseContext parseContext)
    {
        var cursors = parseContext.GetExplicitlyIncludedNamedCursors();
        foreach (var cursor in cursors)
        {
            VisitExplicitlyIncludedName(exploreContext, cursor);
        }
    }

    private void VisitExplicitlyIncludedName(ExploreContext context, clang.CXCursor clangCursor)
    {
#pragma warning disable IDE0072
        var nodeKind = clangCursor.kind switch
#pragma warning restore IDE0072
        {
            clang.CXCursorKind.CXCursor_EnumDecl => CNodeKind.Enum,
            _ => CNodeKind.Unknown
        };

        if (nodeKind == CNodeKind.Unknown)
        {
            // TODO: Add more allowed kinds to explicitly included names
            return;
        }

        var info = context.CreateNodeInfoExplicitlyIncluded(nodeKind, clangCursor);
        context.TryEnqueueNode(info);
    }

    private void VisitIncludes(ExploreContext context, ParseContext parseContext)
    {
        var includeCursors = parseContext.GetIncludes();
        foreach (var includeCursor in includeCursors)
        {
            VisitInclude(context, includeCursor);
        }
    }

    private void VisitInclude(ExploreContext context, clang.CXCursor clangCursor)
    {
        var clangFile = clang.clang_getIncludedFile(clangCursor);
        var stringFile = clang.clang_getFileName(clangFile).String();
        if (string.IsNullOrEmpty(stringFile))
        {
            return;
        }

        var filePath = Path.GetFullPath(stringFile);
        foreach (var systemIncludeDirectory in context.ParseContext.SystemIncludeDirectories)
        {
            if (filePath.Contains(systemIncludeDirectory, StringComparison.InvariantCulture))
            {
                LogSkippedSystemInclude(filePath);
                return;
            }
        }

        if (context.IsIncludeIgnored(filePath))
        {
            LogIgnoredInclude(filePath);
            return;
        }

        if (!_visitedIncludeFilePaths.Add(filePath))
        {
            LogAlreadyVisitedInclude(filePath);
            return;
        }

        var parseContext2 = clangTranslationUnitParser.ParseTranslationUnit(
            filePath,
            context.ParseContext.InputSanitized,
            false,
            true);
        _parseContexts.Add(parseContext2);

        VisitTranslationUnit(context, parseContext2);
    }

    private ExploreContext CreateExploreContext(string filePath, InputSanitizedTargetPlatform input)
    {
#pragma warning disable CA2000
        var parseContext = clangTranslationUnitParser.ParseTranslationUnit(filePath, input);
#pragma warning restore CA2000
        var result = new ExploreContext(services, parseContext);
        return result;
    }

    private void LogFound(CFfiTargetPlatform ffi)
    {
        LogFoundMacroObjects(ffi);
        LogFoundVariables(ffi);
        LogFoundFunctions(ffi);
    }

    private void LogFoundMacroObjects(CFfiTargetPlatform ffi)
    {
        var names = ffi.MacroObjects.Keys.ToArray();
        var namesString = string.Join(", ", names);
        LogFoundMacroObjects(names.Length, namesString);
    }

    private void LogFoundVariables(CFfiTargetPlatform ffi)
    {
        var names = ffi.Variables.Keys.ToArray();
        var namesString = string.Join(", ", names);
        LogFoundVariables(names.Length, namesString);
    }

    private void LogFoundFunctions(CFfiTargetPlatform ffi)
    {
        var names = ffi.Functions.Keys.ToArray();
        var namesString = string.Join(", ", names);
        LogFoundFunctions(names.Length, namesString);
    }

    [LoggerMessage(0, LogLevel.Error, "- Failure")]
    private partial void LogFailure(Exception exception);

    [LoggerMessage(1, LogLevel.Debug, "- Success")]
    private partial void LogSuccess();

    [LoggerMessage(2, LogLevel.Debug, "- Visiting translation unit: {FilePath}")]
    private partial void LogVisitingTranslationUnit(string filePath);

    [LoggerMessage(3, LogLevel.Debug, "- Finished visiting translation unit: {FilePath}")]
    private partial void LogVisitedTranslationUnit(string filePath);

    [LoggerMessage(4, LogLevel.Information, "- Found {FoundCount} macro objects: {Names}")]
    private partial void LogFoundMacroObjects(int foundCount, string names);

    [LoggerMessage(5, LogLevel.Information, "- Found {FoundCount} variables: {Names}")]
    private partial void LogFoundVariables(int foundCount, string names);

    [LoggerMessage(6, LogLevel.Information, "- Found {FoundCount} functions: {Names}")]
    private partial void LogFoundFunctions(int foundCount, string names);

    [LoggerMessage(7, LogLevel.Debug, "- Skipping ignored include file header: {FilePath}")]
    private partial void LogIgnoredInclude(string filePath);

    [LoggerMessage(8, LogLevel.Debug, "- Skipping already visited include file header: {FilePath}")]
    private partial void LogAlreadyVisitedInclude(string filePath);

    [LoggerMessage(9, LogLevel.Debug, "- Skipping system include file header: {FilePath}")]
    private partial void LogSkippedSystemInclude(string filePath);
}
