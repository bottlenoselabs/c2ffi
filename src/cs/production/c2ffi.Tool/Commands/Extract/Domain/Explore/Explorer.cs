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

    private readonly ArrayDeque<ExploreInfoNode> _frontierFunctions = new();
    private readonly ArrayDeque<ExploreInfoNode> _frontierTypes = new();
    private readonly ArrayDeque<ExploreInfoNode> _frontierVariables = new();

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
            ExploreTranslationUnit(context, context.ParseContext);
            ExploreVariables(context);
            ExploreFunctions(context);
            ExploreTypes(context);
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

    private void ExploreVariables(ExploreContext context)
    {
    }

    private void ExploreFunctions(ExploreContext context)
    {
        var totalCount = _frontierFunctions.Count;
        var functionNamesToExplore = string.Join(", ", _frontierFunctions.Select(x => x.Name));
        LogExploringFunctions(totalCount, functionNamesToExplore);
        ExploreFrontier(context, _frontierFunctions);
    }

    private void ExploreTypes(ExploreContext context)
    {
    }

    private void ExploreTranslationUnit(ExploreContext context, ParseContext parseContext)
    {
        LogExploringTranslationUnit(parseContext.FilePath);
        ExploreTopLevelCursors(context, parseContext);
        ExploreIncludeHeaders(context, parseContext);
        LogExploredTranslationUnit(parseContext.FilePath);
    }

    private void ExploreTopLevelCursors(ExploreContext context, ParseContext parseContext)
    {
        var cursors = parseContext.GetTranslationUnitExternalTopLevelCursors();
        foreach (var cursor in cursors)
        {
            ExploreCursor(context, cursor);
        }
    }

    private void ExploreCursor(ExploreContext context, clang.CXCursor cursor)
    {
        var cursorType = clang.clang_getCursorType(cursor);
        if (cursorType.kind == clang.CXTypeKind.CXType_Unexposed)
        {
            // CXType_Unexposed: A type whose specific kind is not exposed via this interface (libclang).
            // When this happens, use the "canonical form" or the standard/normal form of the type
            cursorType = clang.clang_getCanonicalType(cursorType);
        }

        if (cursorType.kind == clang.CXTypeKind.CXType_Attributed)
        {
            // CXTypeKind.CXType_Attributed: The type has a Clang attribute.
            // When this happens, just ignore the attribute and get the actual type.
            cursorType = clang.clang_Type_getModifiedType(cursorType);
        }

        var nodeKind = cursor.kind switch
        {
            clang.CXCursorKind.CXCursor_FunctionDecl => CNodeKind.Function,
            clang.CXCursorKind.CXCursor_VarDecl => CNodeKind.Variable,
            clang.CXCursorKind.CXCursor_EnumDecl => CNodeKind.Enum,
            clang.CXCursorKind.CXCursor_TypedefDecl => CNodeKind.TypeAlias,
            clang.CXCursorKind.CXCursor_StructDecl => CNodeKind.Struct,
            _ => CNodeKind.Unknown
        };

        if (nodeKind == CNodeKind.Unknown)
        {
            LogUnexpectedTopLevelCursor(cursorType.kind.ToString());
            return;
        }

        var cursorName = cursor.Spelling();
        if (context.ParseContext.ExtractOptions.OpaqueTypeNames.Contains(cursorName))
        {
            nodeKind = CNodeKind.OpaqueType;
        }

        var info = context.CreateInfoNode(nodeKind, cursorName, cursor, cursorType, null);
        TryEnqueueExploreInfoNode(context, nodeKind, info);
    }

    private void ExploreIncludeHeaders(ExploreContext context, ParseContext parseContext)
    {
        var includeCursors = parseContext.GetTranslationUnitIncludes();
        foreach (var includeCursor in includeCursors)
        {
            ExploreIncludeHeader(context, includeCursor);
        }
    }

    private void ExploreIncludeHeader(ExploreContext context, clang.CXCursor clangCursor)
    {
        var code = clangCursor.GetCode();
        var isSystemHeader = code.Contains('<', StringComparison.InvariantCulture);
        if (isSystemHeader)
        {
            return;
        }

        var file = clang.clang_getIncludedFile(clangCursor);
        var filePath = Path.GetFullPath(clang.clang_getFileName(file).String());

        if (_visitedIncludeFilePaths.Contains(filePath))
        {
            return;
        }

        _visitedIncludeFilePaths.Add(filePath);

        var parseContext2 = _clangTranslationUnitParser.ParseTranslationUnit(
            filePath,
            context.ParseContext.ExtractOptions,
            false,
            true);
        _parseContexts.Add(parseContext2);

        ExploreTranslationUnit(context, parseContext2);
    }

    private void TryEnqueueExploreInfoNode(ExploreContext context, CNodeKind kind, ExploreInfoNode info)
    {
        var frontier = kind switch
        {
            CNodeKind.Variable => _frontierVariables,
            CNodeKind.Function => _frontierFunctions,
            _ => _frontierTypes
        };

        if (!context.CanExplore(kind, info))
        {
            return;
        }

        LogEnqueueExplore(kind, info.Name, info.Location);
        frontier.PushBack(info);
    }

    private ExploreContext CreateExploreContext(string filePath, ExtractTargetPlatformOptions options)
    {
        var parseContext = _clangTranslationUnitParser.ParseTranslationUnit(filePath, options);
        var result = new ExploreContext(_services, parseContext);
        return result;
    }

    private void ExploreFrontier(ExploreContext context, ArrayDeque<ExploreInfoNode> frontier)
    {
        while (frontier.Count > 0)
        {
            var node = frontier.PopFront()!;
            ExploreNode(context, node);
        }
    }

    private void ExploreNode(ExploreContext context, ExploreInfoNode info)
    {
        var node = context.Explore(info);
        if (node == null)
        {
            return;
        }

        var location = node is CNodeWithLocation nodeWithLocation ? nodeWithLocation.Location : null;
        LogFoundNode(node.NodeKind, node.Name, location);
    }

    private void LogFfi(CFfiTargetPlatform ffi)
    {
        var functionNamesFound = ffi.Functions.Keys.ToArray();
        var functionNamesFoundString = string.Join(", ", functionNamesFound);
        LogFoundFunctions(functionNamesFound.Length, functionNamesFoundString);
    }

    [LoggerMessage(0, LogLevel.Error, "- Expected a top level translation unit declaration (function, variable, enum, typedef, struct, or macro) but found '{KindString}'")]
    private partial void LogUnexpectedTopLevelCursor(string kindString);

    [LoggerMessage(1, LogLevel.Error, "- Failure")]
    private partial void LogFailure(Exception exception);

    [LoggerMessage(2, LogLevel.Debug, "- Success")]
    private partial void LogSuccess();

    [LoggerMessage(3, LogLevel.Debug, "- Exploring translation unit: {FilePath}")]
    private partial void LogExploringTranslationUnit(string filePath);

    [LoggerMessage(4, LogLevel.Information, "- Explored translation unit: {FilePath}")]
    private partial void LogExploredTranslationUnit(string filePath);

    [LoggerMessage(5, LogLevel.Information, "- Exploring macros")]
    private partial void LogExploringMacros();

    [LoggerMessage(6, LogLevel.Information, "- Found {FoundCount} macros: {Names}")]
    private partial void LogFoundMacros(int foundCount, string names);

    [LoggerMessage(7, LogLevel.Information, "- Exploring {Count} variables: {Names}")]
    private partial void LogExploringVariables(int count, string names);

    [LoggerMessage(8, LogLevel.Information, "- Found {FoundCount} variables: {Names}")]
    private partial void LogFoundVariables(int foundCount, string names);

    [LoggerMessage(9, LogLevel.Information, "- Exploring {Count} functions: {Names}")]
    private partial void LogExploringFunctions(int count, string names);

    [LoggerMessage(10, LogLevel.Information, "- Found {FoundCount} functions: {Names}")]
    private partial void LogFoundFunctions(int foundCount, string names);

    [LoggerMessage(11, LogLevel.Information, "- Exploring {Count} types: {Names}")]
    private partial void LogExploringTypes(int count, string names);

    [LoggerMessage(12, LogLevel.Information, "- Found {FoundCount} types: {Names}")]
    private partial void LogFoundTypes(int foundCount, string names);

    [LoggerMessage(13, LogLevel.Debug, "- Enqueued {NodeKind} for exploration '{Name}' ({Location})")]
    private partial void LogEnqueueExplore(CNodeKind nodeKind, string name, CLocation? location);

    [LoggerMessage(14, LogLevel.Information, "- Found {NodeKind} '{Name}' ({Location})")]
    private partial void LogFoundNode(CNodeKind nodeKind, string name, CLocation? location);
}
