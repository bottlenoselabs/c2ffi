// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class ArrayExplorer : NodeExplorer<CArray>
{
    public ArrayExplorer(ILogger<ArrayExplorer> logger)
        : base(logger, false)
    {
    }

    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Either(
        CXTypeKind.CXType_ConstantArray, CXTypeKind.CXType_IncompleteArray);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Array(context, info);
    }

    private static CArray Array(ExploreContext context, ExploreNodeInfo info)
    {
        var type = clang_getElementType(info.Type);
        var typeInfo = context.VisitType(type, info)!;
        var isSystemCursor = context.IsSystemCursor(info.Cursor);

        var result = new CArray
        {
            Name = info.Name,
            TypeInfo = typeInfo,
            IsSystem = isSystemCursor
        };

        return result;
    }
}
