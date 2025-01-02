// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

#pragma warning disable IDE0130
namespace c2ffi.Extract.Explore;
#pragma warning restore IDE0130

[UsedImplicitly]
internal sealed class StructExplorer(ILogger<StructExplorer> logger) : RecordExplorer(logger, false)
{
    protected override KindCursors ExpectedCursors { get; } =
        KindCursors.Is(CXCursorKind.CXCursor_StructDecl);

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return Struct(exploreContext, info);
    }

    private CRecord Struct(ExploreContext exploreContext, NodeInfo info)
    {
        var fields = StructFields(exploreContext, info);
        var comment = exploreContext.Comment(info.ClangCursor);
        var isAnonymous = clang_Cursor_isAnonymous(info.ClangCursor) > 0;

        var record = new CRecord
        {
            RecordKind = CRecordKind.Struct,
            Location = info.Location,
            Name = info.Name,
            Fields = fields,
            SizeOf = info.SizeOf!.Value,
            AlignOf = info.AlignOf!.Value,
            IsAnonymous = isAnonymous,
            Comment = comment
        };

        return record;
    }

    private ImmutableArray<CRecordField> StructFields(
        ExploreContext exploreContext,
        NodeInfo structInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(structInfo.ClangType);
        var fieldCursorsLength = fieldCursors.Length;
        if (fieldCursorsLength > 0)
        {
            for (var i = 0; i < fieldCursors.Length; i++)
            {
                var clangCursor = fieldCursors[i];
                var field = StructField(exploreContext, structInfo, clangCursor);
                builder.Add(field);
            }
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField StructField(
        ExploreContext exploreContext,
        NodeInfo structInfo,
        CXCursor clangCursor)
    {
        var fieldName = exploreContext.GetFieldName(clangCursor);
        var clangType = clang_getCursorType(clangCursor);
        var location = exploreContext.ParseContext.Location(clangCursor);
        var type = exploreContext.VisitType(clangType, structInfo);
        var offsetOf = (int)clang_Cursor_getOffsetOfField(clangCursor) / 8;
        var comment = exploreContext.Comment(clangCursor);

        return new CRecordField
        {
            Name = fieldName,
            Location = location,
            Type = type,
            OffsetOf = offsetOf,
            Comment = comment
        };
    }
}