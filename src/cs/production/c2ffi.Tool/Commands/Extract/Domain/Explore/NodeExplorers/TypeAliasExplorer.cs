// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
internal sealed class TypeAliasExplorer(ILogger<TypeAliasExplorer> logger) : NodeExplorer<CTypeAlias>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_TypedefDecl);

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Typedef);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return TypeAlias(context, info);
    }

    private static CTypeAlias TypeAlias(ExploreContext context, ExploreNodeInfo info)
    {
        var clangAliasType = clang_getTypedefDeclUnderlyingType(info.ClangCursor);
        var underlyingType = context.VisitType(clangAliasType, info);
        var comment = context.Comment(info.ClangCursor);

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
