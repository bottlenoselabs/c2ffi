// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class VariableExplorer(ILogger<VariableExplorer> logger)
    : NodeExplorer<CVariable>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_VarDecl);

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override CNode GetNode(ExploreContext context, ExploreCandidateInfoNode info)
    {
        return Variable(context, info);
    }

    private static CVariable Variable(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var comment = context.Comment(info.Cursor);
        var isSystemCursor = context.IsSystemCursor(info.Cursor);

        var result = new CVariable
        {
            Location = info.Location,
            Name = info.Name,
            Type = info.TypeName,
            Comment = comment,
            IsSystem = isSystemCursor
        };
        return result;
    }
}
