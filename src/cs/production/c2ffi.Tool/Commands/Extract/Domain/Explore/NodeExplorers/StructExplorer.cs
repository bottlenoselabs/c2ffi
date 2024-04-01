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
public sealed class StructExplorer(ILogger<StructExplorer> logger) : RecordExplorer(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_StructDecl);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Struct(context, info);
    }

    private CRecord Struct(ExploreContext context, ExploreNodeInfo info)
    {
        var fields = StructFields(context, info);
        var comment = context.Comment(info.Cursor);

        var record = new CRecord
        {
            RecordKind = CRecordKind.Struct,
            Location = info.Location,
            Name = info.Name,
            Fields = fields,
            SizeOf = info.SizeOf!.Value,
            AlignOf = info.AlignOf!.Value,
            Comment = comment
        };

        return record;
    }

    private ImmutableArray<CRecordField> StructFields(
        ExploreContext context,
        ExploreNodeInfo structInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(structInfo.Type);
        var fieldCursorsLength = fieldCursors.Length;
        if (fieldCursorsLength > 0)
        {
            for (var i = 0; i < fieldCursors.Length; i++)
            {
                var fieldCursor = fieldCursors[i];
                var field = StructField(context, structInfo, fieldCursor);
                builder.Add(field);
            }
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField StructField(
        ExploreContext context,
        ExploreNodeInfo structInfo,
        CXCursor fieldCursor)
    {
        var fieldName = fieldCursor.Spelling();
        var type = clang_getCursorType(fieldCursor);
        var location = context.ParseContext.Location(fieldCursor);
        var typeInfo = context.VisitType(type, structInfo);
        var offsetOf = (int)clang_Cursor_getOffsetOfField(fieldCursor) / 8;
        var comment = context.Comment(fieldCursor);

        return new CRecordField
        {
            Name = fieldName,
            Location = location,
            TypeInfo = typeInfo,
            OffsetOf = offsetOf,
            Comment = comment
        };
    }
}
