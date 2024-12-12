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
internal sealed class UnionExplorer(ILogger<UnionExplorer> logger) : RecordExplorer(logger, false)
{
    protected override KindCursors ExpectedCursors { get; } =
        KindCursors.Is(CXCursorKind.CXCursor_UnionDecl);

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return Union(exploreContext, info);
    }

    private CRecord Union(ExploreContext exploreContext, NodeInfo info)
    {
        var fields = UnionFields(exploreContext, info.ClangType, info);
        var comment = exploreContext.Comment(info.ClangCursor);
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
        ExploreContext exploreContext,
        CXType clangType,
        NodeInfo parentInfo)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(clangType);

        for (var i = 0; i < fieldCursors.Length; i++)
        {
            var clangCursor = fieldCursors[i];
            var field = UnionField(exploreContext, clangCursor, parentInfo);
            builder.Add(field);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField UnionField(
        ExploreContext exploreContext,
        CXCursor clangCursor,
        NodeInfo parentInfo)
    {
        var name = exploreContext.GetFieldName(clangCursor);
        var clangType = clang_getCursorType(clangCursor);
        var location = exploreContext.ParseContext.Location(clangCursor);
        var type = exploreContext.VisitType(clangType, parentInfo);
        var comment = exploreContext.Comment(clangCursor);

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
