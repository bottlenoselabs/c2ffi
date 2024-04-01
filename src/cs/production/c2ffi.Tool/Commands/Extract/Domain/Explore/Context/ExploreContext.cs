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

    public CTypeInfo VisitType(
        clang.CXType clangType,
        ExploreNodeInfo parentInfo,
        CNodeKind? nodeKind = null)
    {
        var clangTypeInfo = ClangTypeInfoProvider.GetTypeInfo(clangType, parentInfo.NodeKind);
        var nodeKindUsed = nodeKind ?? clangTypeInfo.NodeKind;

        var typeInfo = VisitTypeInternal(
            nodeKindUsed,
            clangTypeInfo.Name,
            clangTypeInfo.ClangType,
            clangType,
            clangTypeInfo.ClangCursor,
            parentInfo);
        return typeInfo;
    }

    public string? Comment(clang.CXCursor clangCursor)
    {
        var commentStringC = clang.clang_Cursor_getRawCommentText(clangCursor);
        var commentString = commentStringC.String();
        return string.IsNullOrEmpty(commentString) ? null : commentString;
    }

    public ExploreNodeInfo CreateNodeInfo(
        CNodeKind nodeKind,
        clang.CXCursor clangCursor)
    {
        var cursorName = clangCursor.Spelling();
        var cursorType = clang.clang_getCursorType(clangCursor);
        return CreateNodeInfo(nodeKind, cursorName, clangCursor, cursorType, null);
    }

    public ExploreNodeInfo CreateNodeInfo(
        CNodeKind kind,
        string name,
        clang.CXCursor cursor,
        clang.CXType type,
        ExploreNodeInfo? parentInfo)
    {
        var location = cursor.Location(ParseContext.SystemIncludeDirectories);
        var typeName = type.Spelling();
        var sizeOf = ParseContext.SizeOf(kind, type);
        var alignOf = ParseContext.AlignOf(kind, type);

        var result = new ExploreNodeInfo
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

    private CTypeInfo VisitTypeInternal(
        CNodeKind nodeKind,
        string typeName,
        clang.CXType clangType,
        clang.CXType clangContainerType,
        clang.CXCursor clangCursor,
        ExploreNodeInfo? rootNode)
    {
        var clangCursorLocation = clang.clang_getTypeDeclaration(clangType);
        var location = clangCursorLocation.Location(ParseContext.SystemIncludeDirectories);

        int? sizeOf;
        int? alignOf;
        CTypeInfo? innerType = null;
        if (nodeKind is CNodeKind.Pointer)
        {
            var pointeeTypeCandidate = clang.clang_getPointeeType(clangType);
            var pointeeTypeInfo = ClangTypeInfoProvider.GetTypeInfo(pointeeTypeCandidate, nodeKind);

            innerType = VisitTypeInternal(
                pointeeTypeInfo.NodeKind,
                pointeeTypeInfo.Name,
                pointeeTypeInfo.ClangType,
                pointeeTypeCandidate,
                pointeeTypeInfo.ClangCursor,
                rootNode);
            sizeOf = ParseContext.PointerSize;
            alignOf = ParseContext.PointerSize;
        }
        else if (nodeKind is CNodeKind.Array)
        {
            var elementTypeCandidate = clang.clang_getArrayElementType(clangType);
            var elementTypeInfo = ClangTypeInfoProvider.GetTypeInfo(elementTypeCandidate, nodeKind);

            innerType = VisitTypeInternal(
                elementTypeInfo.NodeKind,
                elementTypeInfo.Name,
                elementTypeInfo.ClangType,
                elementTypeCandidate,
                elementTypeInfo.ClangCursor,
                rootNode);
            sizeOf = ParseContext.PointerSize;
            alignOf = ParseContext.PointerSize;
        }
        else if (nodeKind is CNodeKind.TypeAlias)
        {
            var aliasTypeCandidate = clang.clang_getTypedefDeclUnderlyingType(clangCursor);
            var aliasTypeInfo = ClangTypeInfoProvider.GetTypeInfo(aliasTypeCandidate, nodeKind);

            innerType = VisitTypeInternal(
                aliasTypeInfo.NodeKind,
                aliasTypeInfo.Name,
                aliasTypeInfo.ClangType,
                aliasTypeCandidate,
                aliasTypeInfo.ClangCursor,
                rootNode);
            if (innerType != null)
            {
                if (innerType.NodeKind == CNodeKind.OpaqueType)
                {
                    return innerType;
                }

                sizeOf = innerType.SizeOf;
                alignOf = innerType.AlignOf;
            }
            else
            {
                sizeOf = ParseContext.SizeOf(nodeKind, aliasTypeInfo.ClangType);
                alignOf = ParseContext.AlignOf(nodeKind, aliasTypeInfo.ClangType);
            }
        }
        else
        {
            sizeOf = ParseContext.SizeOf(nodeKind, clangContainerType);
            alignOf = ParseContext.AlignOf(nodeKind, clangContainerType);
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

        var typeInfo = new CTypeInfo
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
            InnerTypeInfo = innerType
        };

        if (typeInfo.Name == "uint64_t" && typeInfo.AlignOf == 4)
        {
            Console.WriteLine();
        }

        if (typeInfo.NodeKind == CNodeKind.TypeAlias && typeInfo.InnerTypeInfo != null &&
            typeInfo.Name == typeInfo.InnerTypeInfo.Name)
        {
            return typeInfo;
        }

        if (location.IsSystem)
        {
            return typeInfo;
        }

        var info = CreateNodeInfo(typeInfo.NodeKind, typeInfo.Name, clangCursor, clangType, rootNode);
        TryEnqueueNode(info);
        return typeInfo;
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
