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
public sealed class UnionExplorer(ILogger<UnionExplorer> logger) : RecordExplorer(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_UnionDecl);

    protected override CNode GetNode(ExploreContext context, ExploreInfoNode info)
    {
        return Union(context, info);
    }

    private CRecord Union(ExploreContext context, ExploreInfoNode info)
    {
        var fields = UnionFields(context, info.Type, info);
        var comment = context.Comment(info.Cursor);

        var cursorLocation = clang_getCursorLocation(info.Cursor);
        var isSystemCursor = clang_Location_isInSystemHeader(cursorLocation) > 0;

        var result = new CRecord
        {
            RecordKind = CRecordKind.Union,
            Location = info.Location,
            Name = info.TypeName,
            Fields = fields,
            SizeOf = info.SizeOf!.Value,
            AlignOf = info.AlignOf!.Value,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }

    private ImmutableArray<CRecordField> UnionFields(
        ExploreContext context,
        CXType type,
        ExploreInfoNode parentInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(type);

        for (var i = 0; i < fieldCursors.Length; i++)
        {
            var fieldCursor = fieldCursors[i];
            var nextRecordField = UnionField(context, fieldCursor, parentInfo, i);
            builder.Add(nextRecordField);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField UnionField(
        ExploreContext context,
        CXCursor cursor,
        ExploreInfoNode parentInfo,
        int fieldIndex)
    {
        var name = cursor.Spelling();
        var type = clang_getCursorType(cursor);
        var location = cursor.Location();
        var typeInfo = context.VisitType(type, parentInfo, fieldIndex: fieldIndex)!;
        var comment = context.Comment(cursor);

        var result = new CRecordField
        {
            Name = name,
            Location = location,
            TypeInfo = typeInfo,
            Comment = comment
        };

        return result;
    }
}
