// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class EnumExplorer(ILogger<EnumExplorer> logger)
    : NodeExplorer<CEnum>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_EnumDecl);

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Enum);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Enum(context, info);
    }

    private CEnum Enum(ExploreContext context, ExploreNodeInfo info)
    {
        var integerType = IntegerTypeInfo(context, info);
        var enumValues = EnumValues(info.ClangCursor);
        var comment = context.Comment(info.ClangCursor);

        var result = new CEnum
        {
            Name = info.Name,
            Location = info.Location,
            SizeOf = integerType.SizeOf!.Value,
            Values = enumValues,
            Comment = comment
        };

        return result;
    }

    private static CType IntegerTypeInfo(ExploreContext context, ExploreNodeInfo info)
    {
        var clangType = clang_getEnumDeclIntegerType(info.ClangCursor);
        return context.VisitType(clangType, info);
    }

    private ImmutableArray<CEnumValue> EnumValues(CXCursor clangCursor)
    {
        var builder = ImmutableArray.CreateBuilder<CEnumValue>();

        var enumValuesCursors = clangCursor.GetDescendents(
            static (child, _) => child.kind == CXCursorKind.CXCursor_EnumConstantDecl);

        foreach (var enumValueCursor in enumValuesCursors)
        {
            var enumValue = CreateEnumValue(enumValueCursor);
            builder.Add(enumValue);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CEnumValue CreateEnumValue(CXCursor clangCursor, string? name = null)
    {
        var value = clang_getEnumConstantDeclValue(clangCursor);
        name ??= clangCursor.Spelling();

        var result = new CEnumValue
        {
            Name = name,
            Value = value
        };

        return result;
    }
}
