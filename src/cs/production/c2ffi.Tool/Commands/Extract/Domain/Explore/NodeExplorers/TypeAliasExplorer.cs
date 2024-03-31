// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public class TypeAliasExplorer : NodeExplorer<CTypeAlias>
{
    public TypeAliasExplorer(ILogger<TypeAliasExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_TypedefDecl);

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Typedef);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return TypeAlias(context, info);
    }

    private static CTypeAlias TypeAlias(ExploreContext context, ExploreNodeInfo info)
    {
        var aliasType = clang_getTypedefDeclUnderlyingType(info.Cursor);
        var aliasTypeInfo = context.VisitType(aliasType, info)!;
        var comment = context.Comment(info.Cursor);
        var isSystemCursor = context.IsSystemCursor(info.Cursor);

        var typedef = new CTypeAlias
        {
            Name = info.Name,
            Location = info.Location,
            UnderlyingTypeInfo = aliasTypeInfo,
            Comment = comment,
            IsSystem = isSystemCursor
        };
        return typedef;
    }
}
