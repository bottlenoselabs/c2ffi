// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using c2ffi.Tool.Commands.Extract.Domain.Parse;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;
using c2ffi.Tool.Internal;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore;

public sealed partial class Explorer
{
    private readonly ILogger<Explorer> _logger;
    private readonly IServiceProvider _services;
    private readonly ClangTranslationUnitParser _clangTranslationUnitParser;

    private readonly List<ParseContext> _parseContexts = new();

    private readonly HashSet<string> _visitedIncludeFilePaths = new();

    public Explorer(ILogger<Explorer> logger, IServiceProvider services, ClangTranslationUnitParser clangTranslationUnitParser)
    {
        _services = services;
        _clangTranslationUnitParser = clangTranslationUnitParser;
        _logger = logger;
    }

    public CFfiTargetPlatform ExtractFfi(
        string filePath,
        ExtractTargetPlatformOptions options)
    {
        using var context = CreateExploreContext(filePath, options);
        using var _ = _logger.BeginScope(options.TargetPlatform)!;

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

    private void VisitTranslationUnit(ExploreContext context, ParseContext parseContext)
    {
        LogVisitingTranslationUnit(parseContext.FilePath);
        VisitIncludes(context, parseContext);
        VisitFunctions(context, parseContext);
        VisitVariables(context, parseContext);
        VisitMacroObjects(context, parseContext);
        LogVisitedTranslationUnit(parseContext.FilePath);
    }

    private void VisitFunctions(ExploreContext context, ParseContext parseContext)
    {
        var functionCursors = parseContext.GetExternalFunctions();
        foreach (var cursor in functionCursors)
        {
            VisitFunction(context, cursor);
        }
    }

    private void VisitFunction(ExploreContext context, clang.CXCursor clangCursor)
    {
        var info = context.CreateNodeInfo(CNodeKind.Function, clangCursor);
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
        var info = context.CreateNodeInfo(CNodeKind.Variable, clangCursor);
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
        var info = context.CreateNodeInfo(CNodeKind.MacroObject, clangCursor);
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
        var file = clang.clang_getIncludedFile(clangCursor);
        var filePath = Path.GetFullPath(clang.clang_getFileName(file).String());
        foreach (var systemIncludeDirectory in context.ParseContext.SystemIncludeDirectories)
        {
            if (filePath.Contains(systemIncludeDirectory, StringComparison.InvariantCulture))
            {
                return;
            }
        }

        if (context.IsIncludeIgnored(filePath))
        {
            LogIgnoreInclude(filePath);
            return;
        }

        if (!_visitedIncludeFilePaths.Add(filePath))
        {
            return;
        }

        var parseContext2 = _clangTranslationUnitParser.ParseTranslationUnit(
            filePath,
            context.ParseContext.ExtractOptions,
            false,
            true);
        _parseContexts.Add(parseContext2);

        VisitTranslationUnit(context, parseContext2);
    }

    private ExploreContext CreateExploreContext(string filePath, ExtractTargetPlatformOptions options)
    {
        var parseContext = _clangTranslationUnitParser.ParseTranslationUnit(filePath, options);
        var result = new ExploreContext(_services, parseContext);
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

    [LoggerMessage(7, LogLevel.Debug, "- Ignored include file header: {FilePath}")]
    private partial void LogIgnoreInclude(string filePath);
}
