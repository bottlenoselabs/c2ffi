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
public sealed class UnionExplorer(ILogger<UnionExplorer> logger) : RecordExplorer(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors { get; } =
        ExploreKindCursors.Is(CXCursorKind.CXCursor_UnionDecl);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return Union(context, info);
    }

    private CRecord Union(ExploreContext context, ExploreNodeInfo info)
    {
        var fields = UnionFields(context, info.Type, info);
        var comment = context.Comment(info.Cursor);

        var result = new CRecord
        {
            RecordKind = CRecordKind.Union,
            Location = info.Location,
            Name = info.TypeName,
            Fields = fields,
            SizeOf = info.SizeOf!.Value,
            AlignOf = info.AlignOf!.Value,
            Comment = comment
        };

        return result;
    }

    private ImmutableArray<CRecordField> UnionFields(
        ExploreContext context,
        CXType type,
        ExploreNodeInfo parentInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(type);

        for (var i = 0; i < fieldCursors.Length; i++)
        {
            var fieldCursor = fieldCursors[i];
            var nextRecordField = UnionField(context, fieldCursor, parentInfo);
            builder.Add(nextRecordField);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField UnionField(
        ExploreContext context,
        CXCursor cursor,
        ExploreNodeInfo parentInfo)
    {
        var name = cursor.Spelling();
        var type = clang_getCursorType(cursor);
        var location = cursor.Location(context.ParseContext.SystemIncludeDirectories);
        var typeInfo = context.VisitType(type, parentInfo)!;
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
