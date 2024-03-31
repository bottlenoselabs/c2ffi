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

    protected override CNode GetNode(ExploreContext context, ExploreCandidateInfoNode info)
    {
        return Enum(context, info);
    }

    private CEnum Enum(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var integerTypeInfo = IntegerTypeInfo(context, info);
        var enumValues = EnumValues(info.Cursor);
        var comment = context.Comment(info.Cursor);
        var isSystemCursor = context.IsSystemCursor(info.Cursor);

        var result = new CEnum
        {
            Name = info.Name,
            Location = info.Location,
            IntegerTypeInfo = integerTypeInfo,
            Values = enumValues,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }

    private static CTypeInfo IntegerTypeInfo(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var integerType = clang_getEnumDeclIntegerType(info.Cursor);
        var typeInfo = context.VisitType(integerType, info)!;
        return typeInfo;
    }

    private ImmutableArray<CEnumValue> EnumValues(CXCursor cursor)
    {
        var builder = ImmutableArray.CreateBuilder<CEnumValue>();

        var enumValuesCursors = cursor.GetDescendents(
            static (child, _) => child.kind == CXCursorKind.CXCursor_EnumConstantDecl);

        foreach (var enumValueCursor in enumValuesCursors)
        {
            var enumValue = CreateEnumValue(enumValueCursor);
            builder.Add(enumValue);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CEnumValue CreateEnumValue(CXCursor cursor, string? name = null)
    {
        var value = clang_getEnumConstantDeclValue(cursor);
        name ??= cursor.Spelling();

        var result = new CEnumValue
        {
            Name = name,
            Value = value
        };

        return result;
    }
}
