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
internal sealed class PointerExplorer(ILogger<PointerExplorer> logger)
    : NodeExplorer<CPointer>(logger, false)
{
    protected override KindCursors ExpectedCursors => KindCursors.Any;

    protected override KindTypes ExpectedTypes { get; } = KindTypes.Is(CXTypeKind.CXType_Pointer);

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        var pointer = Pointer(exploreContext, info);
        return pointer;
    }

    private static CPointer Pointer(ExploreContext exploreContext, NodeInfo info)
    {
        var clangType = clang_getPointeeType(info.ClangType);
        var type = exploreContext.VisitType(clangType, info);
        var comment = exploreContext.Comment(info.ClangCursor);

        var result = new CPointer
        {
            Name = info.Name,
            Type = type,
            Comment = comment
        };

        return result;
    }
}
