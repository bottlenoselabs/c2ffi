// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
internal sealed class VariableExplorer(ILogger<VariableExplorer> logger)
    : NodeExplorer<CVariable>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_VarDecl);

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override bool IsAllowed(ExploreContext context, ExploreNodeInfo info)
    {
        var regexes = context.ParseContext.ExtractInput.IgnoredVariableRegexes;
        foreach (var regex in regexes)
        {
            if (regex.IsMatch(info.Name))
            {
                return false;
            }
        }

        return true;
    }

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Variable(context, info);
    }

    private static CVariable Variable(ExploreContext context, ExploreNodeInfo info)
    {
        var type = context.VisitType(info.ClangType, info);
        var comment = context.Comment(info.ClangCursor);

        var result = new CVariable
        {
            Location = info.Location,
            Name = info.Name,
            Type = type,
            Comment = comment
        };
        return result;
    }
}
