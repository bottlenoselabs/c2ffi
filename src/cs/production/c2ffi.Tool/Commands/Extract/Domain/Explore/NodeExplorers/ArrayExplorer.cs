// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
internal sealed class ArrayExplorer(ILogger<ArrayExplorer> logger) : NodeExplorer<CArray>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Either(
        CXTypeKind.CXType_ConstantArray, CXTypeKind.CXType_IncompleteArray);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Array(context, info);
    }

    private static CArray Array(ExploreContext context, ExploreNodeInfo info)
    {
        var clangElementType = clang_getElementType(info.ClangType);
        var elementType = context.VisitType(clangElementType, info);

        var result = new CArray
        {
            Name = info.Name,
            Type = elementType
        };

        return result;
    }
}
