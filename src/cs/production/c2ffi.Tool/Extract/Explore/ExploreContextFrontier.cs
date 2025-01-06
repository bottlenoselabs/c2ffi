// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace c2ffi.Extract.Explore;

[UsedImplicitly]
internal sealed partial class ExploreContextFrontier(ILogger<ExploreContextFrontier> logger)
{
    private readonly ArrayDeque<NodeInfo> _frontierMacroObjects = new();
    private readonly ArrayDeque<NodeInfo> _frontierVariables = new();
    private readonly ArrayDeque<NodeInfo> _frontierFunctions = new();
    private readonly ArrayDeque<NodeInfo> _frontierTypes = new();

    public void EnqueueNode(NodeInfo info)
    {
        LogEnqueue(info.NodeKind, info.Name, info.Location);
        var frontier = GetFrontier(info);
        frontier.PushBack(info);
    }

    public void Explore(ExploreContext exploreContext)
    {
        ExploreFunctions(exploreContext);
        ExploreVariables(exploreContext);
        ExploreMacroObjects(exploreContext);
        ExploreTypes(exploreContext);
    }

    private void ExploreFunctions(ExploreContext exploreContext)
    {
        var frontier = _frontierFunctions;
        var totalCount = frontier.Count;
        var functionNameCandidates = string.Join(", ", frontier.Select(x => x.Name));
        LogFunctions(totalCount, functionNameCandidates);
        Explore(exploreContext, frontier);
    }

    private void ExploreVariables(ExploreContext exploreContext)
    {
        var frontier = _frontierVariables;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogVariables(totalCount, names);
        Explore(exploreContext, frontier);
    }

    private void ExploreMacroObjects(ExploreContext exploreContext)
    {
        var frontier = _frontierMacroObjects;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogMacroObjects(totalCount, names);
        Explore(exploreContext, frontier);
    }

    private void ExploreTypes(ExploreContext exploreContext)
    {
        var frontier = _frontierTypes;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogTypes(totalCount, names);
        Explore(exploreContext, frontier);
    }

    private ArrayDeque<NodeInfo> GetFrontier(NodeInfo info)
    {
#pragma warning disable IDE0072
        var frontier = info.NodeKind switch
#pragma warning restore IDE0072
        {
            CNodeKind.Variable => _frontierVariables,
            CNodeKind.Function => _frontierFunctions,
            CNodeKind.MacroObject => _frontierMacroObjects,
            _ => _frontierTypes
        };
        return frontier;
    }

    private void Explore(ExploreContext exploreContext, ArrayDeque<NodeInfo> frontier)
    {
        while (frontier.Count > 0)
        {
            var nodeInfo = frontier.PopFront()!;
            var node = exploreContext.Explore(nodeInfo);
            exploreContext.AddNode(node);
        }
    }

    [LoggerMessage(0, LogLevel.Debug, "- Enqueued {NodeKind} for exploration '{Name}' ({Location})")]
    private partial void LogEnqueue(
        CNodeKind nodeKind,
        string name,
        CLocation? location);

    [LoggerMessage(2, LogLevel.Debug, "- Exploring {Count} functions: {Names}")]
    private partial void LogFunctions(int count, string names);

    [LoggerMessage(3, LogLevel.Debug, "- Exploring {Count} variables: {Names}")]
    private partial void LogVariables(int count, string names);

    [LoggerMessage(4, LogLevel.Debug, "- Exploring {Count} macro objects: {Names}")]
    private partial void LogMacroObjects(int count, string names);

    [LoggerMessage(5, LogLevel.Debug, "- Exploring {Count} types: {Names}")]
    private partial void LogTypes(int count, string names);
}
