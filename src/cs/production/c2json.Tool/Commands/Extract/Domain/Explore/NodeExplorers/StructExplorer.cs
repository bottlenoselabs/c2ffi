// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2json.Data;
using c2json.Data.Nodes;
using c2json.Tool.Commands.Extract.Infrastructure.Clang;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2json.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class StructExplorer(ILogger<StructExplorer> logger) : RecordExplorer(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_StructDecl);

    protected override CNode GetNode(ExploreContext context, ExploreInfoNode info)
    {
        return Struct(context, info);
    }

    private CRecord Struct(ExploreContext context, ExploreInfoNode info)
    {
        var fields = StructFields(context, info);
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        try
        {
            var record = new CRecord
            {
                RecordKind = CRecordKind.Struct,
                Location = info.Location,
                Name = info.Name,
                Fields = fields,
                SizeOf = info.SizeOf!.Value,
                AlignOf = info.AlignOf!.Value,
                Comment = comment,
                IsSystem = isSystemCursor
            };

            return record;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private ImmutableArray<CRecordField> StructFields(
        ExploreContext context,
        ExploreInfoNode structInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(structInfo.Type);
        var fieldCursorsLength = fieldCursors.Length;
        if (fieldCursorsLength > 0)
        {
            for (var i = 0; i < fieldCursors.Length; i++)
            {
                var fieldCursor = fieldCursors[i];
                var field = StructField(context, structInfo, fieldCursor, i);
                builder.Add(field);
            }
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField StructField(
        ExploreContext context,
        ExploreInfoNode structInfo,
        CXCursor fieldCursor,
        int fieldIndex)
    {
        var fieldName = fieldCursor.Spelling();
        var type = clang_getCursorType(fieldCursor);
        var location = fieldCursor.Location();
        var typeInfo = context.VisitType(type, structInfo, fieldIndex: fieldIndex)!;
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
