// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;
using c2ffi.Data.Nodes;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore;

public abstract class NodeExplorer<TNode> : NodeExplorer
    where TNode : CNode
{
    protected NodeExplorer(
        ILogger<NodeExplorer<TNode>> logger,
        bool logAlreadyExplored = true)
        : base(logger, logAlreadyExplored)
    {
    }
}

public abstract partial class NodeExplorer
{
    private readonly bool _logAlreadyExplored;
    private readonly ILogger<NodeExplorer> _logger;

    private readonly Dictionary<string, CLocation?> _visitedNodeNames = new();

    protected NodeExplorer(
        ILogger<NodeExplorer> logger,
        bool logAlreadyExplored = true)
    {
        _logger = logger;
        _logAlreadyExplored = logAlreadyExplored;
    }

    protected abstract ExploreKindCursors ExpectedCursors { get; }

    protected abstract ExploreKindTypes ExpectedTypes { get; }

    internal CNode? ExploreInternal(ExploreContext context, ExploreCandidateInfoNode info)
    {
        LogExploring(info.NodeKind, info.Name, info.Location);
        var result = GetNode(context, info);
        if (result == null)
        {
            LogExploreSkipped(info.NodeKind, info.Name, info.Location);
            return null;
        }

        LogExplored(info.NodeKind, info.Name, info.Location);
        return result;
    }

    internal bool CanVisitInternal(ExploreContext context, ExploreCandidateInfoNode info)
    {
        if (!IsExpectedCursor(info))
        {
            LogFailureUnexpectedCursor(info.Cursor.kind);
            return false;
        }

        if (!IsExpectedType(info))
        {
            LogFailureUnexpectedType(info.Type.kind);
            return false;
        }

        if (context.IsSystemCursor(info.Cursor))
        {
            return false;
        }

        if (IsAlreadyVisited(info, out var firstLocation))
        {
            if (_logAlreadyExplored)
            {
                LogAlreadyVisited(info.NodeKind, info.Name, firstLocation);
            }

            return false;
        }

        MarkAsVisited(info);
        return true;
    }

    protected abstract CNode? GetNode(ExploreContext context, ExploreCandidateInfoNode info);

    private bool IsAlreadyVisited(ExploreCandidateInfoNode info, out CLocation? firstLocation)
    {
        var result = _visitedNodeNames.TryGetValue(info.Name, out firstLocation);
        return result;
    }

    private void MarkAsVisited(ExploreCandidateInfoNode info)
    {
        try
        {
            _visitedNodeNames.Add(info.Name, info.Location);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private bool IsExpectedCursor(ExploreCandidateInfoNode info)
    {
        return ExpectedCursors.Matches(info.Cursor.kind);
    }

    private bool IsExpectedType(ExploreCandidateInfoNode info)
    {
        var typeKind = info.Type.kind;
        return ExpectedTypes.Matches(typeKind);
    }

    [LoggerMessage(0, LogLevel.Error, "- Unexpected cursor kind '{CursorKind}'")]
    private partial void LogFailureUnexpectedCursor(CXCursorKind cursorKind);

    [LoggerMessage(1, LogLevel.Error, "- Unexpected type kind '{TypeKind}'")]
    private partial void LogFailureUnexpectedType(CXTypeKind typeKind);

    [LoggerMessage(2, LogLevel.Debug, "- Exploring {NodeKind} '{Name}' ({Location})'")]
    private partial void LogExploring(CNodeKind nodeKind, string name, CLocation? location);

    [LoggerMessage(3, LogLevel.Error, "- Already visited {NodeKind} '{Name}' ({Location})")]
    private partial void LogAlreadyVisited(CNodeKind nodeKind,
        string name,
        CLocation? location);

    [LoggerMessage(4, LogLevel.Debug, "- Exploring {NodeKind} '{Name}' ({Location})'")]
    private partial void LogExplored(CNodeKind nodeKind, string name, CLocation? location);

    [LoggerMessage(5, LogLevel.Debug, "- Skipped exploring {NodeKind} '{Name}' ({Location})'")]
    private partial void LogExploreSkipped(CNodeKind nodeKind,
        string name,
        CLocation? location);
}
