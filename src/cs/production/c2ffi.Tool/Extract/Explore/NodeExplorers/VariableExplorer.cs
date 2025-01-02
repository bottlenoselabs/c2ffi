// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

#pragma warning disable IDE0130
namespace c2ffi.Extract.Explore;
#pragma warning restore IDE0130

[UsedImplicitly]
internal sealed class VariableExplorer(ILogger<VariableExplorer> logger)
    : NodeExplorer<CVariable>(logger, false)
{
    protected override KindCursors ExpectedCursors { get; } =
        KindCursors.Is(CXCursorKind.CXCursor_VarDecl);

    protected override KindTypes ExpectedTypes => KindTypes.Any;

    protected override bool IsIgnored(ExploreContext exploreContext, NodeInfo info)
    {
        var regexes = exploreContext.ParseContext.InputSanitized.IgnoredVariableRegexes;
        foreach (var regex in regexes)
        {
            if (regex.IsMatch(info.Name))
            {
                return false;
            }
        }

        return true;
    }

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return Variable(exploreContext, info);
    }

    private static CVariable Variable(ExploreContext exploreContext, NodeInfo info)
    {
        var type = exploreContext.VisitType(info.ClangType, info);
        var comment = exploreContext.Comment(info.ClangCursor);

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
