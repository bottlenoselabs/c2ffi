// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Clang;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

#pragma warning disable IDE0130
namespace c2ffi.Extract.Explore;
#pragma warning restore IDE0130

[UsedImplicitly]
internal sealed class EnumExplorer(ILogger<EnumExplorer> logger)
    : NodeExplorer<CEnum>(logger, false)
{
    protected override KindCursors ExpectedCursors { get; } =
        KindCursors.Is(CXCursorKind.CXCursor_EnumDecl);

    protected override KindTypes ExpectedTypes { get; } = KindTypes.Is(CXTypeKind.CXType_Enum);

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return Enum(exploreContext, info);
    }

    private CEnum Enum(ExploreContext exploreContext, NodeInfo info)
    {
        var integerType = IntegerTypeInfo(exploreContext, info);
        var enumValues = EnumValues(info.ClangCursor);
        var comment = exploreContext.Comment(info.ClangCursor);

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

    private static CType IntegerTypeInfo(ExploreContext exploreContext, NodeInfo info)
    {
        var clangType = clang_getEnumDeclIntegerType(info.ClangCursor);
        return exploreContext.VisitType(clangType, info);
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
