// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;
using c2ffi.Tool.Commands.Extract.Domain.Parse;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore;

public sealed class ExploreContext : IDisposable
{
    public readonly ParseContext ParseContext;
    private readonly ImmutableDictionary<CNodeKind, NodeExplorer> _nodeHandlers;
    private readonly FfiBuilder _ffiBuilder;
    private readonly ImmutableHashSet<string> _ignoredIncludeFiles;

    public string FilePath => ParseContext.FilePath;

    public ExploreContext(
        IServiceProvider services,
        ParseContext parseContext)
    {
        ParseContext = parseContext;
        _nodeHandlers = GetNodeHandlers(services);
        _ffiBuilder = new FfiBuilder();
        _ignoredIncludeFiles = parseContext.ExtractOptions.IgnoredIncludeFiles.ToImmutableHashSet();
    }

    public CFfiTargetPlatform GetFfi()
    {
        return _ffiBuilder.GetFfi(ParseContext);
    }

    public CNode? Explore(ExploreCandidateInfoNode info)
    {
        var handler = GetHandler(info.NodeKind);
        var node = handler.ExploreInternal(this, info);
        if (node != null)
        {
            _ffiBuilder.AddNode(node);
        }

        return node;
    }

    public bool CanVisit(CNodeKind kind, ExploreCandidateInfoNode node)
    {
        var handler = GetHandler(kind);
        var result = handler.CanVisitInternal(this, node);
        return result;
    }

    public bool IsIncludeIgnored(string filePath)
    {
        return _ignoredIncludeFiles.Contains(filePath);
    }

    public bool IsSystemCursor(clang.CXCursor cursor)
    {
        return ParseContext.IsSystemCursor(cursor);
    }

    public CTypeInfo? GetTypeInfo(clang.CXType type, ExploreCandidateInfoNode info)
    {
        throw new NotImplementedException();
    }

    public CTypeInfo? VisitType(
        clang.CXType clangTypeCandidate,
        ExploreCandidateInfoNode info,
        CNodeKind? nodeKind = null,
        int? fieldIndex = 0)
    {
        var clangTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangTypeCandidate, info.NodeKind);
        var nodeKindUsed = nodeKind ?? clangTypeInfo.NodeKind;

        var rootInfo = info;
        while (rootInfo is { Location: null })
        {
            rootInfo = rootInfo.Parent;
        }

        // var infoNode = CreateInfoNode(
        //     nodeKindUsed,
        //     clangTypeInfo.Name,
        //     clangTypeInfo.ClangCursor,
        //     clangTypeInfo.ClangTypeCanonical,
        //     info);

        var typeInfo = new CTypeInfo
        {
            Name = clangTypeInfo.Name,
            NodeKind = nodeKindUsed
        };
        return typeInfo;
    }

    public string? Comment(clang.CXCursor cursor)
    {
        var commentStringC = clang.clang_Cursor_getRawCommentText(cursor);
        var commentString = commentStringC.String();
        return string.IsNullOrEmpty(commentString) ? null : commentString;
    }

    public ExploreCandidateInfoNode CreateCandidateInfoNode(
        CNodeKind nodeKind,
        clang.CXCursor cursor)
    {
        var cursorName = cursor.Spelling();
        var cursorType = clang.clang_getCursorType(cursor);
        return CreateCandidateInfoNode(nodeKind, cursorName, cursor, cursorType, null);
    }

    public ExploreCandidateInfoNode CreateCandidateInfoNode(
        CNodeKind kind,
        string name,
        clang.CXCursor cursor,
        clang.CXType type,
        ExploreCandidateInfoNode? parentInfo)
    {
        var location = cursor.Location();
        var typeName = type.Spelling();
        var sizeOf = ParseContext.SizeOf(kind, type);
        var alignOf = ParseContext.AlignOf(kind, type);

        var result = new ExploreCandidateInfoNode
        {
            NodeKind = kind,
            Name = name,
            TypeName = typeName,
            Type = type,
            Cursor = cursor,
            Location = location,
            Parent = parentInfo,
            SizeOf = sizeOf,
            AlignOf = alignOf
        };

        return result;
    }

    public void Dispose()
    {
        ParseContext.Dispose();
    }

    private static ImmutableDictionary<CNodeKind, NodeExplorer> GetNodeHandlers(IServiceProvider services)
    {
        var result = new Dictionary<CNodeKind, NodeExplorer>
        {
            { CNodeKind.EnumConstant, services.GetService<EnumConstantExplorer>()! },
            { CNodeKind.Variable, services.GetService<VariableExplorer>()! },
            { CNodeKind.Function, services.GetService<FunctionExplorer>()! },
            { CNodeKind.Struct, services.GetService<StructExplorer>()! },
            { CNodeKind.Union, services.GetService<UnionExplorer>()! },
            { CNodeKind.Enum, services.GetService<EnumExplorer>()! },
            { CNodeKind.TypeAlias, services.GetService<TypeAliasExplorer>()! },
            { CNodeKind.OpaqueType, services.GetService<OpaqueTypeExplorer>()! },
            { CNodeKind.FunctionPointer, services.GetService<FunctionPointerExplorer>()! },
            { CNodeKind.MacroObject, services.GetService<MacroObjectExplorer>()! },
            { CNodeKind.Array, services.GetService<ArrayExplorer>()! },
            { CNodeKind.Pointer, services.GetService<PointerExplorer>()! },
            { CNodeKind.Primitive, services.GetService<PrimitiveExplorer>()! },
        };
        return result.ToImmutableDictionary();
    }

    private NodeExplorer GetHandler(CNodeKind kind)
    {
        var handlerExists = _nodeHandlers.TryGetValue(kind, out var handler);
        if (handlerExists && handler != null)
        {
            return handler;
        }

        var up = new NotImplementedException($"The handler for kind of '{kind}' was not found.");
        throw up;
    }
}
