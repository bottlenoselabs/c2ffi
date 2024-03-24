// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Data.Nodes;
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

    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierMacroObjectCandidates = new();
    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierVariableCandidates = new();
    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierFunctionsCandidates = new();
    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierTypeCandidates = new();

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
            ExploreFrontiers(context);
            result = context.GetFfi();
            LogFfi(result);
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

    private void ExploreFrontiers(ExploreContext context)
    {
        ExploreFrontierFunctions(context);
        ExploreFrontierVariables(context);
        ExploreFrontierMacroObjects(context);
        ExploreFrontierTypes(context);
    }

    private void ExploreFrontierFunctions(ExploreContext context)
    {
        var totalCount = _frontierFunctionsCandidates.Count;
        var functionNameCandidates = string.Join(", ", _frontierFunctionsCandidates.Select(x => x.Name));
        LogExploringFunctionCandidates(totalCount, functionNameCandidates);
        ExploreFrontier(context, _frontierFunctionsCandidates);
    }

    private void ExploreFrontierVariables(ExploreContext context)
    {
        // TODO
    }

    private void ExploreFrontierMacroObjects(ExploreContext context)
    {
        var totalCount = _frontierMacroObjectCandidates.Count;
        var macroCandidateNames = string.Join(", ", _frontierMacroObjectCandidates.Select(x => x.Name));
        LogExploringMacroObjectCandidates(totalCount, macroCandidateNames);
        ExploreFrontier(context, _frontierMacroObjectCandidates);
    }

    private void ExploreFrontierTypes(ExploreContext context)
    {
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
        var info = context.CreateCandidateInfoNode(CNodeKind.Function, clangCursor);
        TryEnqueueCandidateInfoNode(context, info);
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
        // TODO
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
        var info = context.CreateCandidateInfoNode(CNodeKind.MacroObject, clangCursor);
        TryEnqueueCandidateInfoNode(context, info);
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
        var code = clangCursor.GetCode();
        var isSystemHeader = code.Contains('<', StringComparison.InvariantCulture);
        if (isSystemHeader)
        {
            return;
        }

        var file = clang.clang_getIncludedFile(clangCursor);
        var filePath = Path.GetFullPath(clang.clang_getFileName(file).String());

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

    private void TryEnqueueCandidateInfoNode(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var frontier = info.NodeKind switch
        {
            CNodeKind.Variable => _frontierVariableCandidates,
            CNodeKind.Function => _frontierFunctionsCandidates,
            CNodeKind.MacroObject => _frontierMacroObjectCandidates,
            _ => _frontierTypeCandidates
        };

        if (!context.CanVisit(info.NodeKind, info))
        {
            return;
        }

        LogEnqueueCandidate(info.NodeKind, info.Name, info.Location);
        frontier.PushBack(info);
    }

    private ExploreContext CreateExploreContext(string filePath, ExtractTargetPlatformOptions options)
    {
        var parseContext = _clangTranslationUnitParser.ParseTranslationUnit(filePath, options);
        var result = new ExploreContext(_services, parseContext);
        return result;
    }

    private void ExploreFrontier(ExploreContext context, ArrayDeque<ExploreCandidateInfoNode> frontier)
    {
        while (frontier.Count > 0)
        {
            var node = frontier.PopFront()!;
            ExploreNode(context, node);
        }
    }

    private void ExploreNode(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var node = context.Explore(info);
        if (node == null)
        {
            return;
        }

        var location = node is CNodeWithLocation nodeWithLocation ? nodeWithLocation.Location : null;
        LogExploredNode(node.NodeKind, node.Name, location);
    }

    private void LogFfi(CFfiTargetPlatform ffi)
    {
        var functionNamesFound = ffi.Functions.Keys.ToArray();
        var functionNamesFoundString = string.Join(", ", functionNamesFound);
        LogFoundFunctions(functionNamesFound.Length, functionNamesFoundString);
    }

    [LoggerMessage(0, LogLevel.Error, "- Failure")]
    private partial void LogFailure(Exception exception);

    [LoggerMessage(1, LogLevel.Debug, "- Success")]
    private partial void LogSuccess();

    [LoggerMessage(2, LogLevel.Debug, "- Visiting translation unit: {FilePath}")]
    private partial void LogVisitingTranslationUnit(string filePath);

    [LoggerMessage(3, LogLevel.Information, "- Finished visiting translation unit: {FilePath}")]
    private partial void LogVisitedTranslationUnit(string filePath);

    [LoggerMessage(4, LogLevel.Information, "- Exploring {Count} macro object candidates: {Names}")]
    private partial void LogExploringMacroObjectCandidates(int count, string names);

    [LoggerMessage(5, LogLevel.Information, "- Found {FoundCount} macro objects: {Names}")]
    private partial void LogFoundMacroObjects(int foundCount, string names);

    [LoggerMessage(6, LogLevel.Information, "- Exploring {Count} variable candidates: {Names}")]
    private partial void LogExploringVariableCandidates(int count, string names);

    [LoggerMessage(7, LogLevel.Information, "- Found {FoundCount} variables: {Names}")]
    private partial void LogFoundVariables(int foundCount, string names);

    [LoggerMessage(8, LogLevel.Information, "- Exploring {Count} function candidates: {Names}")]
    private partial void LogExploringFunctionCandidates(int count, string names);

    [LoggerMessage(9, LogLevel.Information, "- Found {FoundCount} functions: {Names}")]
    private partial void LogFoundFunctions(int foundCount, string names);

    [LoggerMessage(10, LogLevel.Information, "- Exploring {Count} type candidates: {Names}")]
    private partial void LogExploringTypeCandidates(int count, string names);

    [LoggerMessage(11, LogLevel.Information, "- Found {FoundCount} types: {Names}")]
    private partial void LogFoundTypes(int foundCount, string names);

    [LoggerMessage(12, LogLevel.Debug, "- Enqueued {NodeKind} candidate for exploration '{Name}' ({Location})")]
    private partial void LogEnqueueCandidate(CNodeKind nodeKind, string name, CLocation? location);

    [LoggerMessage(13, LogLevel.Information, "- Explored {NodeKind} '{Name}' ({Location})")]
    private partial void LogExploredNode(CNodeKind nodeKind, string name, CLocation? location);

    [LoggerMessage(14, LogLevel.Information, "- Ignored include file header: {FilePath}")]
    private partial void LogIgnoreInclude(string filePath);
}
