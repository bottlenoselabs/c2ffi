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

    protected override CNode GetNode(ExploreContext context, NodeInfo info)
    {
        return Union(context, info);
    }

    private CRecord Union(ExploreContext context, NodeInfo info)
    {
        var nestedRecords = ImmutableArray.CreateBuilder<CRecord>();
        var fields = UnionFields(context, info.ClangType, info, nestedRecords);
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
            IsAnonymous = isAnonymous,
            NestedRecords = nestedRecords.ToImmutable()
        };

        return record;
    }

    private ImmutableArray<CRecordField> UnionFields(
        ExploreContext exploreContext,
        CXType clangType,
        NodeInfo parentInfo,
        ImmutableArray<CRecord>.Builder nestedRecords)
    {
        var builder = ImmutableArray.CreateBuilder<CRecordField>();
        var fieldCursors = FieldCursors(clangType);

        for (var i = 0; i < fieldCursors.Length; i++)
        {
            var clangCursor = fieldCursors[i];
            var field = UnionField(exploreContext, clangCursor, parentInfo, nestedRecords);
            builder.Add(field);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private CRecordField UnionField(
        ExploreContext context,
        CXCursor clangCursor,
        NodeInfo parentInfo,
        ImmutableArray<CRecord>.Builder nestedRecords)
    {
        var name = context.GetFieldName(clangCursor);
        var clangType = clang_getCursorType(clangCursor);
        var location = context.ParseContext.Location(clangCursor, out _);
        var type = context.VisitType(clangType, parentInfo);
        var comment = context.Comment(clangCursor);

        if ((type.IsAnonymous ?? false) && type.NodeKind is CNodeKind.Union or CNodeKind.Struct)
        {
            var clangCursorRecordNested = clang_getTypeDeclaration(clangType);
            var nodeInfo =
                context.CreateNodeInfoRecordNested(type.NodeKind, type.Name, clangCursorRecordNested, clangType, parentInfo);
            var node = (CRecord)context.Explore(nodeInfo)!;
            nestedRecords.Add(node);
        }

        var unionField = new CRecordField
        {
            Name = name,
            Location = location,
            Type = type,
            Comment = comment
        };

        return unionField;
    }
}
