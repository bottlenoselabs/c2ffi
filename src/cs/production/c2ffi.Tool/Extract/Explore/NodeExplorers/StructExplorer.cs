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

    protected override CNode GetNode(ExploreContext context, NodeInfo info)
    {
        return Struct(context, info);
    }

    private CRecord Struct(ExploreContext context, NodeInfo info)
    {
        var nestedRecords = ImmutableArray.CreateBuilder<CRecord>();
        var fields = StructFields(context, info, nestedRecords);
        var comment = context.Comment(info.ClangCursor);
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
            Comment = comment,
            NestedRecords = nestedRecords.ToImmutable()
        };

        return record;
    }

    private ImmutableArray<CRecordField> StructFields(
        ExploreContext context,
        NodeInfo structInfo,
        ImmutableArray<CRecord>.Builder nestedRecords)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(structInfo.ClangType);
        var fieldCursorsLength = fieldCursors.Length;
        if (fieldCursorsLength > 0)
        {
            for (var i = 0; i < fieldCursors.Length; i++)
            {
                var clangCursor = fieldCursors[i];
                var field = StructField(context, structInfo, clangCursor, nestedRecords);
                builder.Add(field);
            }
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField StructField(
        ExploreContext context,
        NodeInfo structInfo,
        CXCursor clangCursor,
        ImmutableArray<CRecord>.Builder nestedRecords)
    {
        var fieldName = context.GetFieldName(clangCursor);
        var clangType = clang_getCursorType(clangCursor);
        var location = context.ParseContext.Location(clangCursor, out _);
        var type = context.VisitType(clangType, structInfo);
        var offsetOf = (int)clang_Cursor_getOffsetOfField(clangCursor) / 8;
        var comment = context.Comment(clangCursor);

        if ((type.IsAnonymous ?? false) && type.NodeKind is CNodeKind.Union or CNodeKind.Struct)
        {
            var clangCursorRecordNested = clang_getTypeDeclaration(clangType);
            var nodeInfo =
                context.CreateNodeInfoRecordNested(type.NodeKind, type.Name, clangCursorRecordNested, clangType, structInfo);
            var node = (CRecord)context.Explore(nodeInfo)!;
            nestedRecords.Add(node);
        }

        var structField = new CRecordField
        {
            Name = fieldName,
            Location = location,
            Type = type,
            OffsetOf = offsetOf,
            Comment = comment
        };

        return structField;
    }
}
