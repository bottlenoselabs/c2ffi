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
internal sealed class TypeAliasExplorer(ILogger<TypeAliasExplorer> logger) : NodeExplorer<CTypeAlias>(logger, false)
{
    protected override KindCursors ExpectedCursors { get; } =
        KindCursors.Is(CXCursorKind.CXCursor_TypedefDecl);

    protected override KindTypes ExpectedTypes { get; } = KindTypes.Is(CXTypeKind.CXType_Typedef);

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return TypeAlias(exploreContext, info);
    }

    private static CTypeAlias TypeAlias(ExploreContext exploreContext, NodeInfo info)
    {
        var clangAliasType = clang_getTypedefDeclUnderlyingType(info.ClangCursor);
        var underlyingType = exploreContext.VisitType(clangAliasType, info);
        var comment = exploreContext.Comment(info.ClangCursor);

        var typeAlias = new CTypeAlias
        {
            Name = info.Name,
            Location = info.Location,
            UnderlyingType = underlyingType,
            Comment = comment
        };
        return typeAlias;
    }
}
