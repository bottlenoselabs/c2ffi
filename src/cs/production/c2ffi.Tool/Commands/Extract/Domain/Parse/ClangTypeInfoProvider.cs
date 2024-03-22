// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Parse;

#pragma warning disable CA1815
#pragma warning disable CA1034

public static class ClangTypeInfoProvider
{
    public struct ClangTypeInfo
    {
        public string Name;
        public CNodeKind NodeKind;
        public clang.CXType ClangTypeCanonical;
        public clang.CXCursor ClangCursor;

        public static ClangTypeInfo Create(CNodeKind nodeKind, clang.CXType clangTypeCanonical)
        {
            var result = default(ClangTypeInfo);

            result.Name = clangTypeCanonical.Spelling();
            result.NodeKind = nodeKind;
            result.ClangTypeCanonical = clangTypeCanonical;
            result.ClangCursor = clang.clang_getTypeDeclaration(clangTypeCanonical);

            return result;
        }
    }

    public static ClangTypeInfo GetTypeInfo(
        clang.CXType clangType,
        CNodeKind? parentNodeKind = null)
    {
        var clangCursor = clang.clang_getTypeDeclaration(clangType);
        var clangCursorType = clangCursor.kind != clang.CXCursorKind.CXCursor_NoDeclFound
            ? clang.clang_getCursorType(clangCursor)
            : clangType;

        if (IsPrimitive(clangCursorType))
        {
            return TypeKindPrimitive(clangCursorType);
        }

        switch (clangCursorType.kind)
        {
            case clang.CXTypeKind.CXType_Attributed:
                return TypeKindAttributed(parentNodeKind, clangCursorType);
            case clang.CXTypeKind.CXType_Elaborated:
                return TypeKindElaborated(parentNodeKind, clangCursorType);
            case clang.CXTypeKind.CXType_ConstantArray:
            case clang.CXTypeKind.CXType_IncompleteArray:
                return TypeKindArray(clangCursorType);
            case clang.CXTypeKind.CXType_Unexposed:
                return TypeKindUnexposed(parentNodeKind, clangCursorType);
            case clang.CXTypeKind.CXType_Pointer:
                return TypeKindPointer(clangCursorType);
            case clang.CXTypeKind.CXType_Enum:
                return TypeKindEnum(clangCursorType);
            case clang.CXTypeKind.CXType_Record:
                return TypeKindRecord(clangCursorType, clangCursor.kind);
            case clang.CXTypeKind.CXType_Typedef:
                return TypeKindTypeAlias(parentNodeKind, clangCursor, clangCursorType);
            case clang.CXTypeKind.CXType_FunctionNoProto or clang.CXTypeKind.CXType_FunctionProto:
                return TypeKindFunction(parentNodeKind, clangCursor, clangCursorType);
        }

        var up = new InvalidOperationException($"Unknown Clang type kind '{clangType.kind}'.");
        throw up;
    }

    private static ClangTypeInfo TypeKindPrimitive(clang.CXType clangCursorType)
    {
        return ClangTypeInfo.Create(CNodeKind.Primitive, clangCursorType);
    }

    private static ClangTypeInfo TypeKindAttributed(
        CNodeKind? parentNodeKind,
        clang.CXType clangCursorType)
    {
        var clangType = clang.clang_Type_getModifiedType(clangCursorType);
        return GetTypeInfo(clangType, parentNodeKind);
    }

    private static ClangTypeInfo TypeKindElaborated(
        CNodeKind? parentNodeKind,
        clang.CXType clangCursorType)
    {
        var clangType = clang.clang_Type_getNamedType(clangCursorType);
        return GetTypeInfo(clangType, parentNodeKind);
    }

    private static ClangTypeInfo TypeKindUnexposed(
        CNodeKind? parentNodeKind,
        clang.CXType clangCursorType)
    {
        var clangTypeCanonical = clang.clang_getCanonicalType(clangCursorType);
        return GetTypeInfo(clangTypeCanonical, parentNodeKind);
    }

    private static ClangTypeInfo TypeKindArray(
        clang.CXType clangCursorType)
    {
        return ClangTypeInfo.Create(CNodeKind.Array, clangCursorType);
    }

    private static ClangTypeInfo TypeKindPointer(
        clang.CXType clangCursorType)
    {
        // ReSharper disable once IdentifierTypo
        var clangTypePointee = clang.clang_getPointeeType(clangCursorType);
        if (clangTypePointee.kind == clang.CXTypeKind.CXType_Attributed)
        {
            clangTypePointee = clang.clang_Type_getModifiedType(clangTypePointee);
        }

        if (clangTypePointee.kind is
            clang.CXTypeKind.CXType_FunctionProto or
            clang.CXTypeKind.CXType_FunctionNoProto)
        {
            return ClangTypeInfo.Create(CNodeKind.FunctionPointer, clangTypePointee);
        }

        return ClangTypeInfo.Create(CNodeKind.Pointer, clangCursorType);
    }

    private static ClangTypeInfo TypeKindEnum(
        clang.CXType clangCursorType)
    {
        return ClangTypeInfo.Create(CNodeKind.Enum, clangCursorType);
    }

    private static ClangTypeInfo TypeKindRecord(
        clang.CXType clangCursorType,
        clang.CXCursorKind clangCursorKind)
    {
        var sizeOf = clang.clang_Type_getSizeOf(clangCursorType);
        if (sizeOf == -2)
        {
            return ClangTypeInfo.Create(CNodeKind.OpaqueType, clangCursorType);
        }

        var kind = clangCursorKind == clang.CXCursorKind.CXCursor_StructDecl ? CNodeKind.Struct : CNodeKind.Union;
        return ClangTypeInfo.Create(kind, clangCursorType);
    }

    private static ClangTypeInfo TypeKindTypeAlias(
        CNodeKind? parentNodeKind,
        clang.CXCursor clangCursor,
        clang.CXType clangCursorType)
    {
        var clangTypeUnderlying = clang.clang_getTypedefDeclUnderlyingType(clangCursor);
        if (clangTypeUnderlying.kind == clang.CXTypeKind.CXType_Pointer)
        {
            return ClangTypeInfo.Create(CNodeKind.TypeAlias, clangCursorType);
        }

        var x = GetTypeInfo(clangTypeUnderlying, parentNodeKind);
        var sizeOf = clang.clang_Type_getSizeOf(x.ClangTypeCanonical);
        var kind = sizeOf == -2 ? CNodeKind.OpaqueType : CNodeKind.TypeAlias;
        return ClangTypeInfo.Create(kind, clangCursorType);
    }

    private static ClangTypeInfo TypeKindFunction(
        CNodeKind? parentNodeKind,
        clang.CXCursor clangCursor,
        clang.CXType clangCursorType)
    {
        if (clangCursor.kind == clang.CXCursorKind.CXCursor_NoDeclFound)
        {
            return ClangTypeInfo.Create(CNodeKind.FunctionPointer, clangCursorType);
        }

        return parentNodeKind == CNodeKind.TypeAlias
            ? ClangTypeInfo.Create(CNodeKind.FunctionPointer, clangCursorType)
            : ClangTypeInfo.Create(CNodeKind.Function, clangCursorType);
    }

    private static bool IsPrimitive(clang.CXType type)
    {
        return type.kind switch
        {
            clang.CXTypeKind.CXType_Void => true,
            clang.CXTypeKind.CXType_Bool => true,
            clang.CXTypeKind.CXType_Char_S => true,
            clang.CXTypeKind.CXType_SChar => true,
            clang.CXTypeKind.CXType_Char_U => true,
            clang.CXTypeKind.CXType_UChar => true,
            clang.CXTypeKind.CXType_UShort => true,
            clang.CXTypeKind.CXType_UInt => true,
            clang.CXTypeKind.CXType_ULong => true,
            clang.CXTypeKind.CXType_ULongLong => true,
            clang.CXTypeKind.CXType_Short => true,
            clang.CXTypeKind.CXType_Int => true,
            clang.CXTypeKind.CXType_Long => true,
            clang.CXTypeKind.CXType_LongLong => true,
            clang.CXTypeKind.CXType_Float => true,
            clang.CXTypeKind.CXType_Double => true,
            clang.CXTypeKind.CXType_LongDouble => true,
            _ => false
        };
    }
}
