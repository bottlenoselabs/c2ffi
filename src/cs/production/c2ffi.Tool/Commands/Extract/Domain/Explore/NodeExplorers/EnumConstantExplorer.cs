// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Globalization;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public class EnumConstantExplorer(ILogger<EnumConstantExplorer> logger)
    : NodeExplorer<CEnumConstant>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_EnumConstantDecl);

    protected override ExploreKindTypes ExpectedTypes { get; } =
        ExploreKindTypes.Either(CXTypeKind.CXType_Int, CXTypeKind.CXType_UInt, CXTypeKind.CXType_ULong);

    protected override CNode GetNode(ExploreContext context, ExploreCandidateInfoNode info)
    {
        return EnumConstant(context, info);
    }

    private CEnumConstant EnumConstant(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var typeInfo = context.VisitType(info.Type, info)!;
        var value = clang_getEnumConstantDeclValue(info.Cursor).ToString(CultureInfo.InvariantCulture);
        var comment = context.Comment(info.Cursor);
        var isSystemCursor = context.IsSystemCursor(info.Cursor);

        var result = new CEnumConstant
        {
            Name = info.Name,
            Location = info.Location ?? info.Parent!.Location,
            TypeInfo = typeInfo,
            Value = value,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }
}
