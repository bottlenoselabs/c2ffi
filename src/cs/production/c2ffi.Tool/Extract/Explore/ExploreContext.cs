// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using bottlenoselabs;
using c2ffi.Clang;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Extract.Parse;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Extract.Explore;

internal sealed class ExploreContext(
    IServiceProvider services,
    ParseContext parseContext) : IDisposable
{
    public readonly ParseContext ParseContext = parseContext;
    private readonly ImmutableDictionary<CNodeKind, NodeExplorer> _nodeHandlers = GetNodeHandlers(services);
    private readonly ExploreContextFfiBuilder _ffiBuilder = new();
    private readonly ExploreContextFrontier _frontier = services.GetService<ExploreContextFrontier>()!;

    private readonly ImmutableHashSet<string> _ignoredIncludeFiles = [.. parseContext.InputSanitized.IgnoredIncludeFiles];

    public CFfiTargetPlatform GetFfi()
    {
        _frontier.Explore(this);
        return _ffiBuilder.GetFfi(ParseContext);
    }

    public void TryEnqueueNode(NodeInfo info)
    {
        var handler = GetHandler(info.NodeKind);
        if (!handler.CanVisitInternal(info))
        {
            return;
        }

        _frontier.EnqueueNode(info);
    }

    public CNode? Explore(NodeInfo info)
    {
        var handler = GetHandler(info.NodeKind);
        return handler.ExploreInternal(this, info);
    }

    public void AddNode(CNode? node)
    {
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
        NodeInfo? parentInfo,
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

    public NodeInfo CreateNodeInfoFunction(clang.CXCursor clangCursor)
    {
        var clangCursorName = clangCursor.Spelling();
        var clangCursorType = clang.clang_getCursorType(clangCursor);
        var clangTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangCursorType);

        var nodeInfo = CreateNodeInfo(
            CNodeKind.Function,
            clangCursorName,
            clangTypeInfo.Name,
            clangCursor,
            clangTypeInfo.ClangType,
            null);
        return nodeInfo;
    }

    public NodeInfo CreateNodeInfoMacroObject(clang.CXCursor clangCursor)
    {
        var clangCursorName = clangCursor.Spelling();
        var clangCursorType = clang.clang_getCursorType(clangCursor);

        var nodeInfo = CreateNodeInfo(
            CNodeKind.MacroObject,
            clangCursorName,
            clangCursorType.Spelling(),
            clangCursor,
            clangCursorType,
            null);
        return nodeInfo;
    }

    public NodeInfo CreateNodeInfoVariable(clang.CXCursor clangCursor)
    {
        var clangCursorName = clangCursor.Spelling();
        var clangCursorType = clang.clang_getCursorType(clangCursor);
        var clangTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangCursorType);

        var nodeInfo = CreateNodeInfo(
            CNodeKind.Variable,
            clangCursorName,
            clangTypeInfo.Name,
            clangCursor,
            clangTypeInfo.ClangType,
            null);
        return nodeInfo;
    }

    public NodeInfo CreateNodeInfoRecordNested(
        CNodeKind nodeKind,
        string typeName,
        clang.CXCursor clangCursor,
        clang.CXType clangType,
        NodeInfo parentInfo)
    {
        var clangCursorName = clangCursor.Spelling();

        var nodeInfo = CreateNodeInfo(
            nodeKind,
            clangCursorName,
            typeName,
            clangCursor,
            clangType,
            parentInfo);
        return nodeInfo;
    }

    public NodeInfo CreateNodeInfo(clang.CXCursor clangCursor)
    {
        var clangCursorName = clangCursor.Spelling();

        var clangCursorType = clang.clang_getCursorType(clangCursor);
        var clangTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangCursorType);

        var nodeInfo = CreateNodeInfo(
            clangTypeInfo.NodeKind,
            clangCursorName,
            clangTypeInfo.Name,
            clangCursor,
            clangTypeInfo.ClangType,
            null);
        return nodeInfo;
    }

    public void Dispose()
    {
        ParseContext.Dispose();
    }

    public string GetFieldName(clang.CXCursor clangCursor)
    {
        if (clangCursor.kind != clang.CXCursorKind.CXCursor_FieldDecl)
        {
            return string.Empty;
        }

        var name = clangCursor.Spelling();

        // NOTE: In newer versions of Clang (specifically found on 18.1), getting the name of the field has changed if it anonymous. Old behavior was to return empty string.
        if (name.Contains("::(anonymous at", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return name;
    }

    private NodeInfo CreateNodeInfo(
        CNodeKind kind,
        string name,
        string typeName,
        clang.CXCursor clangCursor,
        clang.CXType clangType,
        NodeInfo? parentInfo)
    {
        var location = ParseContext.Location(clangCursor, out _);
        var sizeOf = ParseContext.SizeOf(kind, clangType);
        var alignOf = ParseContext.AlignOf(kind, clangType);

        var result = new NodeInfo
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

    private CType VisitTypeInternal(
        CNodeKind nodeKind,
        string typeName,
        clang.CXType clangType,
        clang.CXType clangContainerType,
        clang.CXCursor clangCursor,
        NodeInfo? parentInfo)
    {
        var clangCursorLocation = clang.clang_getTypeDeclaration(clangType);
        var location = ParseContext.Location(clangCursorLocation, out _);

        int? sizeOf;
        int? alignOf;
        CType? innerType = null;
#pragma warning disable IDE0010
        switch (nodeKind)
#pragma warning restore IDE0010
        {
            case CNodeKind.Pointer:
                innerType = VisitTypeInternalPointer(nodeKind, clangType, parentInfo);
                sizeOf = ParseContext.PointerSize;
                alignOf = ParseContext.PointerSize;
                break;
            case CNodeKind.Array:
                innerType = VisitTypeInternalArray(nodeKind, clangType, parentInfo);
                sizeOf = ParseContext.PointerSize;
                alignOf = ParseContext.PointerSize;
                break;
            case CNodeKind.TypeAlias:
                innerType = VisitTypeInternalTypeAlias(nodeKind, clangCursor, parentInfo);
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

        if (nodeKind is CNodeKind.Union or CNodeKind.Struct && isAnonymous)
        {
            return type;
        }

        if (nodeKind != CNodeKind.FunctionPointer && location.IsSystem)
        {
            return type;
        }

        var isBlocked = !ParseContext.InputSanitized.IsNameAllowed(typeName);
        if (isBlocked)
        {
            return type;
        }

        var info = CreateNodeInfo(type.NodeKind, type.Name, type.Name, clangCursor, clangType, parentInfo);
        TryEnqueueNode(info);
        return type;
    }

    private CType VisitTypeInternalPointer(
        CNodeKind nodeKind,
        clang.CXType clangType,
        NodeInfo? parentInfo)
    {
        var pointeeTypeCandidate = clang.clang_getPointeeType(clangType);
        var pointeeTypeInfo = ClangTypeInfoProvider.GetTypeInfo(pointeeTypeCandidate, nodeKind);

        var innerType = VisitTypeInternal(
            pointeeTypeInfo.NodeKind,
            pointeeTypeInfo.Name,
            pointeeTypeInfo.ClangType,
            pointeeTypeCandidate,
            pointeeTypeInfo.ClangCursor,
            parentInfo);
        return innerType;
    }

    private CType VisitTypeInternalArray(
        CNodeKind nodeKind,
        clang.CXType clangType,
        NodeInfo? parentInfo)
    {
        var elementTypeCandidate = clang.clang_getArrayElementType(clangType);
        var elementTypeInfo = ClangTypeInfoProvider.GetTypeInfo(elementTypeCandidate, nodeKind);

        var innerType = VisitTypeInternal(
            elementTypeInfo.NodeKind,
            elementTypeInfo.Name,
            elementTypeInfo.ClangType,
            elementTypeCandidate,
            elementTypeInfo.ClangCursor,
            parentInfo);
        return innerType;
    }

    private CType VisitTypeInternalTypeAlias(
        CNodeKind nodeKind,
        clang.CXCursor clangCursor,
        NodeInfo? parentInfo)
    {
        var aliasTypeCandidate = clang.clang_getTypedefDeclUnderlyingType(clangCursor);
        var aliasTypeInfo = ClangTypeInfoProvider.GetTypeInfo(aliasTypeCandidate, nodeKind);

        var innerType = VisitTypeInternal(
            aliasTypeInfo.NodeKind,
            aliasTypeInfo.Name,
            aliasTypeInfo.ClangType,
            aliasTypeCandidate,
            aliasTypeInfo.ClangCursor,
            parentInfo);

        return innerType;
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
