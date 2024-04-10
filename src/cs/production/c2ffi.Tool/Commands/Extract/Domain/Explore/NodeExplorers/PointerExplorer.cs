// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class PointerExplorer(ILogger<PointerExplorer> logger)
    : NodeExplorer<CPointer>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Pointer);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        var pointer = Pointer(context, info);
        return pointer;
    }

    private static CPointer Pointer(ExploreContext context, ExploreNodeInfo info)
    {
        var clangType = clang_getPointeeType(info.ClangType);
        var type = context.VisitType(clangType, info);
        var comment = context.Comment(info.ClangCursor);

        var result = new CPointer
        {
            Name = info.Name,
            Type = type,
            Comment = comment
        };

        return result;
    }
}
