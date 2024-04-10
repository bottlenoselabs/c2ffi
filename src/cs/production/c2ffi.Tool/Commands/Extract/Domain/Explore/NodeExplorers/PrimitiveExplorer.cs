// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class PrimitiveExplorer(ILogger<PrimitiveExplorer> logger)
    : NodeExplorer<CPrimitive>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Either(
        CXTypeKind.CXType_Void,
        CXTypeKind.CXType_Bool,
        CXTypeKind.CXType_Char_U,
        CXTypeKind.CXType_UChar,
        CXTypeKind.CXType_Char16,
        CXTypeKind.CXType_Char32,
        CXTypeKind.CXType_UShort,
        CXTypeKind.CXType_UInt,
        CXTypeKind.CXType_ULong,
        CXTypeKind.CXType_ULongLong,
        CXTypeKind.CXType_UInt128,
        CXTypeKind.CXType_Char_S,
        CXTypeKind.CXType_SChar,
        CXTypeKind.CXType_WChar,
        CXTypeKind.CXType_Short,
        CXTypeKind.CXType_Int,
        CXTypeKind.CXType_Long,
        CXTypeKind.CXType_LongLong,
        CXTypeKind.CXType_Int128,
        CXTypeKind.CXType_Float,
        CXTypeKind.CXType_Double,
        CXTypeKind.CXType_LongDouble);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Primitive(context, info);
    }

    private static CPrimitive Primitive(ExploreContext context, ExploreNodeInfo info)
    {
        var type = context.VisitType(info.ClangType, info);
        var comment = context.Comment(info.ClangCursor);

        var result = new CPrimitive
        {
            Name = info.Name,
            Type = type,
            Comment = comment
        };
        return result;
    }
}
