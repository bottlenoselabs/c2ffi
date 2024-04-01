// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
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

    protected override bool IsAllowed(ExploreContext context, ExploreNodeInfo info)
    {
        var ignoredVariables = context.ParseContext.ExtractOptions.IgnoredVariables;
        return ignoredVariables.IsEmpty || !ignoredVariables.Contains(info.Name);
    }

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Variable(context, info);
    }

    private static CVariable Variable(ExploreContext context, ExploreNodeInfo info)
    {
        var typeInfo = context.VisitType(info.Type, info);
        var comment = context.Comment(info.Cursor);

        var result = new CVariable
        {
            Location = info.Location,
            Name = info.Name,
            TypeInfo = typeInfo,
            Comment = comment
        };
        return result;
    }
}
