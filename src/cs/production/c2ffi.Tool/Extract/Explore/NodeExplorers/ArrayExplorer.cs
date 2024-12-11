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
internal sealed class ArrayExplorer(ILogger<ArrayExplorer> logger) : NodeExplorer<CArray>(logger, false)
{
    protected override KindCursors ExpectedCursors => KindCursors.Any;

    protected override KindTypes ExpectedTypes { get; } = KindTypes.Either(
        CXTypeKind.CXType_ConstantArray, CXTypeKind.CXType_IncompleteArray);

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return Array(exploreContext, info);
    }

    private static CArray Array(ExploreContext exploreContext, NodeInfo info)
    {
        var clangElementType = clang_getElementType(info.ClangType);
        var elementType = exploreContext.VisitType(clangElementType, info);

        var result = new CArray
        {
            Name = info.Name,
            Type = elementType
        };

        return result;
    }
}
