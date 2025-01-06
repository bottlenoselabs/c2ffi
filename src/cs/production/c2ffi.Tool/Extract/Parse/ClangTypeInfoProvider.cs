// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs;
using c2ffi.Clang;
using c2ffi.Data;

namespace c2ffi.Extract.Parse;

#pragma warning disable CA1815
#pragma warning disable CA1034

internal static class ClangTypeInfoProvider
{
    internal struct ClangTypeInfo
    {
        public string Name;
        public CNodeKind NodeKind;
        public clang.CXType ClangType;
        public clang.CXCursor ClangCursor;

        public static ClangTypeInfo Create(CNodeKind nodeKind, clang.CXType clangTypeCanonical)
        {
            var result = default(ClangTypeInfo);
            var clangCursor = clang.clang_getTypeDeclaration(clangTypeCanonical);

            result.Name = GetTypeName(clangTypeCanonical, clangCursor);
            result.NodeKind = nodeKind;
            result.ClangType = clangTypeCanonical;
            result.ClangCursor = clangCursor;

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

        if (clangCursorType.IsPrimitive())
        {
            return GetTypeInfoPrimitive(clangCursorType);
        }

#pragma warning disable IDE0010
        switch (clangCursorType.kind)
#pragma warning restore IDE0010
        {
            case clang.CXTypeKind.CXType_Attributed:
                return GetTypeInfoAttributed(parentNodeKind, clangCursorType);
            case clang.CXTypeKind.CXType_Elaborated:
                return GetTypeInfoElaborated(parentNodeKind, clangCursorType);
            case clang.CXTypeKind.CXType_ConstantArray:
            case clang.CXTypeKind.CXType_IncompleteArray:
                return GetTypeInfoArray(clangCursorType);
            case clang.CXTypeKind.CXType_Unexposed:
                return GetTypeInfoUnexposed(parentNodeKind, clangCursorType);
            case clang.CXTypeKind.CXType_Pointer:
                return GetTypeInfoPointer(clangCursorType);
            case clang.CXTypeKind.CXType_Enum:
                return GetTypeInfoEnum(clangCursorType);
            case clang.CXTypeKind.CXType_Record:
                return GetTypeInfoRecord(clangCursorType, clangCursor.kind);
            case clang.CXTypeKind.CXType_Typedef:
                return GetTypeInfoAlias(clangCursorType);
            case clang.CXTypeKind.CXType_FunctionNoProto or clang.CXTypeKind.CXType_FunctionProto:
                return GetTypeInfoFunction(parentNodeKind, clangCursor, clangCursorType);
        }

        var up = new InvalidOperationException($"Unknown Clang type kind '{clangType.kind}'.");
        throw up;
    }

    private static ClangTypeInfo GetTypeInfoPrimitive(clang.CXType clangCursorType)
    {
        return ClangTypeInfo.Create(CNodeKind.Primitive, clangCursorType);
    }

    private static ClangTypeInfo GetTypeInfoAttributed(
        CNodeKind? parentNodeKind,
        clang.CXType clangCursorType)
    {
        var clangType = clang.clang_Type_getModifiedType(clangCursorType);
        return GetTypeInfo(clangType, parentNodeKind);
    }

    private static ClangTypeInfo GetTypeInfoElaborated(
        CNodeKind? parentNodeKind,
        clang.CXType clangCursorType)
    {
        var clangType = clang.clang_Type_getNamedType(clangCursorType);
        return GetTypeInfo(clangType, parentNodeKind);
    }

    private static ClangTypeInfo GetTypeInfoUnexposed(
        CNodeKind? parentNodeKind,
        clang.CXType clangCursorType)
    {
        var clangTypeCanonical = clang.clang_getCanonicalType(clangCursorType);
        return GetTypeInfo(clangTypeCanonical, parentNodeKind);
    }

    private static ClangTypeInfo GetTypeInfoArray(
        clang.CXType clangCursorType)
    {
        return ClangTypeInfo.Create(CNodeKind.Array, clangCursorType);
    }

    private static ClangTypeInfo GetTypeInfoPointer(
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

    private static ClangTypeInfo GetTypeInfoEnum(
        clang.CXType clangCursorType)
    {
        return ClangTypeInfo.Create(CNodeKind.Enum, clangCursorType);
    }

    private static ClangTypeInfo GetTypeInfoRecord(
        clang.CXType clangCursorType,
        clang.CXCursorKind clangCursorKind)
    {
        var sizeOf = clang.clang_Type_getSizeOf(clangCursorType);
        // CXTypeLayoutError_Incomplete = -2
        if (sizeOf == -2)
        {
            return ClangTypeInfo.Create(CNodeKind.OpaqueType, clangCursorType);
        }

        var kind = clangCursorKind == clang.CXCursorKind.CXCursor_StructDecl ? CNodeKind.Struct : CNodeKind.Union;
        return ClangTypeInfo.Create(kind, clangCursorType);
    }

    private static ClangTypeInfo GetTypeInfoAlias(clang.CXType clangCursorType)
    {
        // var clangTypeUnderlying = clang.clang_getTypedefDeclUnderlyingType(clangCursor);
        // var clangTypeUnderlyingInfo = GetTypeInfo(clangTypeUnderlying, parentNodeKind);

        return ClangTypeInfo.Create(CNodeKind.TypeAlias, clangCursorType);
    }

    private static ClangTypeInfo GetTypeInfoFunction(
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

    private static string GetTypeName(clang.CXType clangType, clang.CXCursor clangCursor)
    {
        var isAnonymous = clang.clang_Cursor_isAnonymous(clangCursor) > 0;
        var isRecord = clangCursor.kind is clang.CXCursorKind.CXCursor_StructDecl or clang.CXCursorKind.CXCursor_UnionDecl;
        if (isRecord && isAnonymous)
        {
            return string.Empty;
        }

        return clangType.Spelling();
    }
}
