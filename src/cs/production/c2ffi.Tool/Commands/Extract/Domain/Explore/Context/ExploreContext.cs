// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;
using c2ffi.Tool.Commands.Extract.Domain.Parse;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.Context;

public sealed class ExploreContext : IDisposable
{
    public readonly ParseContext ParseContext;
    private readonly ImmutableDictionary<CNodeKind, NodeExplorer> _nodeHandlers;
    private readonly ExploreFfiBuilder _ffiBuilder;
    private readonly ExploreFrontier _frontier;

    private readonly ImmutableHashSet<string> _ignoredIncludeFiles;

    public ExploreContext(
        IServiceProvider services,
        ParseContext parseContext)
    {
        ParseContext = parseContext;
        _nodeHandlers = GetNodeHandlers(services);
        _ffiBuilder = new ExploreFfiBuilder();
        _frontier = services.GetService<ExploreFrontier>()!;
        _ignoredIncludeFiles = parseContext.ExtractOptions.IgnoredIncludeFiles.ToImmutableHashSet();
    }

    public CFfiTargetPlatform GetFfi()
    {
        _frontier.Explore(this);
        return _ffiBuilder.GetFfi(ParseContext);
    }

    public void TryEnqueueNode(ExploreNodeInfo info)
    {
        var handler = GetHandler(info.NodeKind);
        if (!handler.CanVisitInternal(this, info))
        {
            return;
        }

        _frontier.EnqueueNode(info);
    }

    public void TryExplore(ExploreNodeInfo info)
    {
        var handler = GetHandler(info.NodeKind);
        var node = handler.ExploreInternal(this, info);
        if (node != null)
        {
            _ffiBuilder.AddNode(node);
        }
    }

    public bool IsIncludeIgnored(string filePath)
    {
        return _ignoredIncludeFiles.Contains(filePath);
    }

    public CType VisitType(
        clang.CXType clangType,
        ExploreNodeInfo? parentInfo,
        CNodeKind? nodeKind = null)
    {
        var typeInfo = ClangTypeInfoProvider.GetTypeInfo(clangType, parentInfo?.NodeKind);
        var nodeKindUsed = nodeKind ?? typeInfo.NodeKind;

        var type = VisitTypeInternal(
            nodeKindUsed,
            typeInfo.Name,
            typeInfo.ClangType,
            clangType,
            typeInfo.ClangCursor,
            parentInfo);

        return type;
    }

    public string? Comment(clang.CXCursor clangCursor)
    {
        var commentStringC = clang.clang_Cursor_getRawCommentText(clangCursor);
        var commentString = commentStringC.String();
        return string.IsNullOrEmpty(commentString) ? null : commentString;
    }

    public ExploreNodeInfo CreateTopLevelNodeInfo(
        CNodeKind nodeKind,
        clang.CXCursor clangCursor)
    {
        var clangCursorName = clangCursor.Spelling();
        var clangCursorType = clang.clang_getCursorType(clangCursor);

        if (nodeKind == CNodeKind.MacroObject)
        {
            return CreateNodeInfo(
                nodeKind,
                clangCursorName,
                clangCursorType.Spelling(),
                clangCursor,
                clangCursorType,
                null);
        }

        var clangTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangCursorType);

        return CreateNodeInfo(
            nodeKind,
            clangCursorName,
            clangTypeInfo.Name,
            clangCursor,
            clangTypeInfo.ClangType,
            null);
    }

    public void Dispose()
    {
        ParseContext.Dispose();
    }

    private CType VisitTypeInternal(
        CNodeKind nodeKind,
        string typeName,
        clang.CXType clangType,
        clang.CXType clangContainerType,
        clang.CXCursor clangCursor,
        ExploreNodeInfo? rootNode)
    {
        var clangCursorLocation = clang.clang_getTypeDeclaration(clangType);
        var location = ParseContext.Location(clangCursorLocation);

        int? sizeOf;
        int? alignOf;
        CType? innerType = null;
        switch (nodeKind)
        {
            case CNodeKind.Pointer:
                innerType = VisitTypeInternalPointer(nodeKind, clangType, rootNode);
                sizeOf = ParseContext.PointerSize;
                alignOf = ParseContext.PointerSize;
                break;
            case CNodeKind.Array:
                innerType = VisitTypeInternalArray(nodeKind, clangType, rootNode);
                sizeOf = ParseContext.PointerSize;
                alignOf = ParseContext.PointerSize;
                break;
            case CNodeKind.TypeAlias:
                innerType = VisitTypeInternalTypeAlias(nodeKind, clangCursor, rootNode);
                sizeOf = innerType.SizeOf;
                alignOf = innerType.AlignOf;
                break;
            default:
                sizeOf = ParseContext.SizeOf(nodeKind, clangContainerType);
                alignOf = ParseContext.AlignOf(nodeKind, clangContainerType);
                break;
        }

        var arraySizeValue = (int)clang.clang_getArraySize(clangContainerType);
        int? arraySize = arraySizeValue >= 0 ? arraySizeValue : null;

        int? elementSize = null;
        if (nodeKind == CNodeKind.Array)
        {
            var arrayTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangType, nodeKind);
            var elementType = clang.clang_getElementType(arrayTypeInfo.ClangType);
            elementSize = ParseContext.SizeOf(arrayTypeInfo.NodeKind, elementType);

            if (clangType.kind == clang.CXTypeKind.CXType_ConstantArray)
            {
                sizeOf = arraySize!.Value * elementSize!.Value;
            }
        }

        var isAnonymous = clang.clang_Cursor_isAnonymous(clangCursorLocation) > 0;

        var type = new CType
        {
            Name = typeName,
            NodeKind = nodeKind,
            SizeOf = sizeOf,
            AlignOf = alignOf,
            ElementSize = elementSize,
            ArraySizeOf = arraySize,
            Location = location,
            IsAnonymous = isAnonymous ? true : null,
            IsConst = false,
            InnerType = innerType
        };

        if (type is { NodeKind: CNodeKind.TypeAlias, InnerType: not null } &&
            type.Name == type.InnerType.Name)
        {
            return type;
        }

        if (location.IsSystem && nodeKind != CNodeKind.FunctionPointer)
        {
            return type;
        }

        var info = CreateNodeInfo(type.NodeKind, type.Name, type.Name, clangCursor, clangType, rootNode);
        TryEnqueueNode(info);
        return type;
    }

    private CType VisitTypeInternalPointer(
        CNodeKind nodeKind,
        clang.CXType clangType,
        ExploreNodeInfo? rootNode)
    {
        var pointeeTypeCandidate = clang.clang_getPointeeType(clangType);
        var pointeeTypeInfo = ClangTypeInfoProvider.GetTypeInfo(pointeeTypeCandidate, nodeKind);

        var innerType = VisitTypeInternal(
            pointeeTypeInfo.NodeKind,
            pointeeTypeInfo.Name,
            pointeeTypeInfo.ClangType,
            pointeeTypeCandidate,
            pointeeTypeInfo.ClangCursor,
            rootNode);
        return innerType;
    }

    private CType VisitTypeInternalArray(
        CNodeKind nodeKind,
        clang.CXType clangType,
        ExploreNodeInfo? rootNode)
    {
        var elementTypeCandidate = clang.clang_getArrayElementType(clangType);
        var elementTypeInfo = ClangTypeInfoProvider.GetTypeInfo(elementTypeCandidate, nodeKind);

        var innerType = VisitTypeInternal(
            elementTypeInfo.NodeKind,
            elementTypeInfo.Name,
            elementTypeInfo.ClangType,
            elementTypeCandidate,
            elementTypeInfo.ClangCursor,
            rootNode);
        return innerType;
    }

    private CType VisitTypeInternalTypeAlias(
        CNodeKind nodeKind,
        clang.CXCursor clangCursor,
        ExploreNodeInfo? rootNode)
    {
        var aliasTypeCandidate = clang.clang_getTypedefDeclUnderlyingType(clangCursor);
        var aliasTypeInfo = ClangTypeInfoProvider.GetTypeInfo(aliasTypeCandidate, nodeKind);

        var innerType = VisitTypeInternal(
            aliasTypeInfo.NodeKind,
            aliasTypeInfo.Name,
            aliasTypeInfo.ClangType,
            aliasTypeCandidate,
            aliasTypeInfo.ClangCursor,
            rootNode);

        return innerType;
    }

    private ExploreNodeInfo CreateNodeInfo(
        CNodeKind kind,
        string name,
        string typeName,
        clang.CXCursor clangCursor,
        clang.CXType clangType,
        ExploreNodeInfo? parentInfo)
    {
        var location = ParseContext.Location(clangCursor);
        var sizeOf = ParseContext.SizeOf(kind, clangType);
        var alignOf = ParseContext.AlignOf(kind, clangType);

        var result = new ExploreNodeInfo
        {
            NodeKind = kind,
            Name = name,
            TypeName = typeName,
            ClangType = clangType,
            ClangCursor = clangCursor,
            Location = location,
            Parent = parentInfo,
            SizeOf = sizeOf,
            AlignOf = alignOf
        };

        return result;
    }

    private static ImmutableDictionary<CNodeKind, NodeExplorer> GetNodeHandlers(IServiceProvider services)
    {
        var result = new Dictionary<CNodeKind, NodeExplorer>
        {
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
