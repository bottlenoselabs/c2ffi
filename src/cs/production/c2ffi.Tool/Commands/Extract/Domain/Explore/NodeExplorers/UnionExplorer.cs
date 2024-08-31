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
        var fields = UnionFields(context, info.ClangType, info);
        var comment = context.Comment(info.ClangCursor);
        var isAnonymous = clang_Cursor_isAnonymous(info.ClangCursor) > 0;

        var record = new CRecord
        {
            RecordKind = CRecordKind.Union,
            Location = info.Location,
            Name = info.TypeName,
            Fields = fields,
            SizeOf = info.SizeOf!.Value,
            AlignOf = info.AlignOf!.Value,
            Comment = comment,
            IsAnonymous = isAnonymous
        };

        return record;
    }

    private ImmutableArray<CRecordField> UnionFields(
        ExploreContext context,
        CXType clangType,
        ExploreNodeInfo parentInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(clangType);

        for (var i = 0; i < fieldCursors.Length; i++)
        {
            var clangCursor = fieldCursors[i];
            var field = UnionField(context, clangCursor, parentInfo);
            builder.Add(field);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField UnionField(
        ExploreContext context,
        CXCursor clangCursor,
        ExploreNodeInfo parentInfo)
    {
        var name = context.GetFieldName(clangCursor);
        var clangType = clang_getCursorType(clangCursor);
        var location = context.ParseContext.Location(clangCursor);
        var type = context.VisitType(clangType, parentInfo);
        var comment = context.Comment(clangCursor);

        var result = new CRecordField
        {
            Name = name,
            Location = location,
            Type = type,
            Comment = comment
        };

        return result;
    }
}
