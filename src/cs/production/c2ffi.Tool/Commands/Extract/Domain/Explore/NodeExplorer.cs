// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore;

internal abstract class NodeExplorer<TNode>(
    ILogger<NodeExplorer<TNode>> logger,
    bool logAlreadyExplored = true) : NodeExplorer(logger, logAlreadyExplored)
    where TNode : CNode;

internal abstract partial class NodeExplorer(
    ILogger<NodeExplorer> logger,
    bool logAlreadyExplored = true)
{
    private readonly Dictionary<string, CLocation?> _visitedNodeNames = [];

    protected abstract ExploreKindCursors ExpectedCursors { get; }

    protected abstract ExploreKindTypes ExpectedTypes { get; }

    internal CNode? ExploreInternal(ExploreContext context, ExploreNodeInfo info)
    {
        LogExploring(info.NodeKind.ToString(), info.Name, info.Location);
        CNode? result;

        try
        {
            result = GetNode(context, info);
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            LogFailureExplore(e, info.NodeKind.ToString(), info.Name, info.Location);
            return null;
        }

        LogExplored(info.NodeKind.ToString(), info.Name, info.Location);
        return result;
    }

    internal bool CanVisitInternal(ExploreContext context, ExploreNodeInfo info)
    {
        if (!IsExpectedCursor(info))
        {
            LogFailureUnexpectedCursor(info.ClangCursor.kind);
            return false;
        }

        if (!IsExpectedType(info))
        {
            LogFailureUnexpectedType(info.ClangType.kind);
            return false;
        }

        if (info.Location.IsSystem && info.NodeKind != CNodeKind.FunctionPointer)
        {
            return false;
        }

        if (IsAlreadyVisited(info, out var firstLocation))
        {
            if (logAlreadyExplored)
            {
                LogAlreadyVisited(info.NodeKind.ToString(), info.Name, firstLocation);
            }

            return false;
        }

        if (!IsAllowed(context, info))
        {
            LogNotAllowed(info.NodeKind.ToString(), info.Name);
            return false;
        }

        MarkAsVisited(info);
        return true;
    }

    protected abstract CNode? GetNode(ExploreContext context, ExploreNodeInfo info);

    protected virtual bool IsAllowed(ExploreContext context, ExploreNodeInfo info)
    {
        return true;
    }

    private bool IsAlreadyVisited(ExploreNodeInfo info, out CLocation? firstLocation)
    {
        var result = _visitedNodeNames.TryGetValue(info.Name, out firstLocation);
        return result;
    }

    private void MarkAsVisited(ExploreNodeInfo info)
    {
        _visitedNodeNames.Add(info.Name, info.Location);
    }

    private bool IsExpectedCursor(ExploreNodeInfo info)
    {
        return ExpectedCursors.Matches(info.ClangCursor.kind);
    }

    private bool IsExpectedType(ExploreNodeInfo info)
    {
        var typeKind = info.ClangType.kind;
        return ExpectedTypes.Matches(typeKind);
    }

    [LoggerMessage(0, LogLevel.Error, "- Unexpected cursor kind '{CursorKind}'")]
    private partial void LogFailureUnexpectedCursor(CXCursorKind cursorKind);

    [LoggerMessage(1, LogLevel.Error, "- Unexpected type kind '{TypeKind}'")]
    private partial void LogFailureUnexpectedType(CXTypeKind typeKind);

    [LoggerMessage(2, LogLevel.Error, "- Failed to explore {Kind} '{Name}' ({Location})'")]
    private partial void LogFailureExplore(Exception e, string kind, string name, CLocation? location);

    [LoggerMessage(3, LogLevel.Debug, "- Already visited {Kind} '{Name}' ({Location})")]
    private partial void LogAlreadyVisited(string kind, string name, CLocation? location);

    [LoggerMessage(4, LogLevel.Debug, "- Exploring {Kind} '{Name}' ({Location})'")]
    private partial void LogExploring(string kind, string name, CLocation? location);

    [LoggerMessage(5, LogLevel.Debug, "- Explored {Kind} '{Name}' ({Location})'")]
    private partial void LogExplored(string kind, string name, CLocation? location);

    [LoggerMessage(6, LogLevel.Debug, "- {Kind} '{Name}' not allowed, skipping")]
    private partial void LogNotAllowed(string kind, string name);
}
