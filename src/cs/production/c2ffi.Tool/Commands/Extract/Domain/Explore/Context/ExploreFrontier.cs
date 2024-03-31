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

    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierMacroObjectCandidates = new();
    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierVariableCandidates = new();
    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierFunctionsCandidates = new();
    private readonly ArrayDeque<ExploreCandidateInfoNode> _frontierTypeCandidates = new();

    public ExploreFrontier(ILogger<ExploreFrontier> logger)
    {
        _logger = logger;
    }

    public void EnqueueCandidate(ExploreCandidateInfoNode info)
    {
        LogEnqueueCandidate(info.NodeKind, info.Name, info.Location);
        var frontier = GetFrontier(info);
        frontier.PushBack(info);
    }

    public void Explore(ExploreContext context)
    {
        ExploreFunctionsCandidates(context);
        ExploreVariableCandidates(context);
        ExploreMacroObjectCandidates(context);
        ExploreTypeCandidates(context);
    }

    private void ExploreFunctionsCandidates(ExploreContext context)
    {
        var frontier = _frontierFunctionsCandidates;
        var totalCount = frontier.Count;
        var functionNameCandidates = string.Join(", ", frontier.Select(x => x.Name));
        LogFunctionCandidates(totalCount, functionNameCandidates);
        ExploreCandidates(context, frontier);
    }

    private void ExploreVariableCandidates(ExploreContext context)
    {
        var frontier = _frontierVariableCandidates;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogVariableCandidates(totalCount, names);
        ExploreCandidates(context, frontier);
    }

    private void ExploreMacroObjectCandidates(ExploreContext context)
    {
        var frontier = _frontierMacroObjectCandidates;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogMacroObjectCandidates(totalCount, names);
        ExploreCandidates(context, frontier);
    }

    private void ExploreTypeCandidates(ExploreContext context)
    {
        var frontier = _frontierTypeCandidates;
        var totalCount = frontier.Count;
        var names = string.Join(", ", frontier.Select(x => x.Name));
        LogTypeCandidates(totalCount, names);
        ExploreCandidates(context, frontier);
    }

    private ArrayDeque<ExploreCandidateInfoNode> GetFrontier(ExploreCandidateInfoNode info)
    {
        var frontier = info.NodeKind switch
        {
            CNodeKind.Variable => _frontierVariableCandidates,
            CNodeKind.Function => _frontierFunctionsCandidates,
            CNodeKind.MacroObject => _frontierMacroObjectCandidates,
            _ => _frontierTypeCandidates
        };
        return frontier;
    }

    private void ExploreCandidates(ExploreContext context, ArrayDeque<ExploreCandidateInfoNode> frontier)
    {
        while (frontier.Count > 0)
        {
            var node = frontier.PopFront()!;
            ExploreCandidate(context, node);
        }
    }

    private void ExploreCandidate(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var node = context.TryExploreCandidate(info);
        if (node == null)
        {
            return;
        }

        var location = node is CNodeWithLocation nodeWithLocation ? nodeWithLocation.Location : null;
        LogExploredNode(node.NodeKind, node.Name, location);
    }

    [LoggerMessage(0, LogLevel.Information, "- Enqueued {NodeKind} candidate for exploration '{Name}' ({Location})")]
    private partial void LogEnqueueCandidate(CNodeKind nodeKind,
        string name,
        CLocation? location);

    [LoggerMessage(1, LogLevel.Information, "- Explored {NodeKind} '{Name}' ({Location})")]
    private partial void LogExploredNode(CNodeKind nodeKind, string name, CLocation? location);

    [LoggerMessage(2, LogLevel.Information, "- Exploring {Count} function candidates: {Names}")]
    private partial void LogFunctionCandidates(int count, string names);

    [LoggerMessage(3, LogLevel.Information, "- Exploring {Count} variable candidates: {Names}")]
    private partial void LogVariableCandidates(int count, string names);

    [LoggerMessage(4, LogLevel.Information, "- Exploring {Count} macro object candidates: {Names}")]
    private partial void LogMacroObjectCandidates(int count, string names);

    [LoggerMessage(5, LogLevel.Information, "- Exploring {Count} type candidates: {Names}")]
    private partial void LogTypeCandidates(int count, string names);
}
