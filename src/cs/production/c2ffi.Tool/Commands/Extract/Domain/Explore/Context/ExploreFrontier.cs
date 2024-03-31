// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Internal;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.Context;

public sealed partial class ExploreFrontier
{
    private readonly ILogger<ExploreFrontier> _logger;

    private readonly ArrayDeque<ExploreNodeInfo> _frontierMacroObjects = new();
    private readonly ArrayDeque<ExploreNodeInfo> _frontierVariables = new();
    private readonly ArrayDeque<ExploreNodeInfo> _frontierFunctions = new();
    private readonly ArrayDeque<ExploreNodeInfo> _frontierTypes = new();

    public ExploreFrontier(ILogger<ExploreFrontier> logger)
    {
        _logger = logger;
    }

    public void EnqueueNode(ExploreNodeInfo info)
    {
        LogEnqueue(info.NodeKind, info.Name, info.Location);
        var frontier = GetFrontier(info);
        frontier.PushBack(info);
    }

    public void Explore(ExploreContext context)
    {
        ExploreFunctions(context);
        ExploreVariables(context);
        ExploreMacroObjects(context);
        ExploreTypes(context);
    }

    private void ExploreFunctions(ExploreContext context)
    {
        var frontier = _frontierFunctions;
        var totalCount = frontier.Count;
        var functionNameCandidates = string.Join(", ", frontier.Select(x => x.Name));
        LogFunctions(totalCount, functionNameCandidates);
        Explore(context, frontier);
    }

    private void ExploreVariables(ExploreContext context)
    {
        var frontier = _frontierVariables;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogVariables(totalCount, names);
        Explore(context, frontier);
    }

    private void ExploreMacroObjects(ExploreContext context)
    {
        var frontier = _frontierMacroObjects;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogMacroObjects(totalCount, names);
        Explore(context, frontier);
    }

    private void ExploreTypes(ExploreContext context)
    {
        var frontier = _frontierTypes;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogTypes(totalCount, names);
        Explore(context, frontier);
    }

    private ArrayDeque<ExploreNodeInfo> GetFrontier(ExploreNodeInfo info)
    {
        var frontier = info.NodeKind switch
        {
            CNodeKind.Variable => _frontierVariables,
            CNodeKind.Function => _frontierFunctions,
            CNodeKind.MacroObject => _frontierMacroObjects,
            _ => _frontierTypes
        };
        return frontier;
    }

    private void Explore(ExploreContext context, ArrayDeque<ExploreNodeInfo> frontier)
    {
        while (frontier.Count > 0)
        {
            var node = frontier.PopFront()!;
            context.TryExplore(node);
        }
    }

    [LoggerMessage(0, LogLevel.Information, "- Enqueued {NodeKind} for exploration '{Name}' ({Location})")]
    private partial void LogEnqueue(
        CNodeKind nodeKind,
        string name,
        CLocation? location);

    [LoggerMessage(2, LogLevel.Information, "- Exploring {Count} functions: {Names}")]
    private partial void LogFunctions(int count, string names);

    [LoggerMessage(3, LogLevel.Information, "- Exploring {Count} variables: {Names}")]
    private partial void LogVariables(int count, string names);

    [LoggerMessage(4, LogLevel.Information, "- Exploring {Count} macro objects: {Names}")]
    private partial void LogMacroObjects(int count, string names);

    [LoggerMessage(5, LogLevel.Information, "- Exploring {Count} types: {Names}")]
    private partial void LogTypes(int count, string names);
}
