// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using bottlenoselabs;
using c2json.Data;
using c2json.Tool.Commands.Extract.Infrastructure.Clang;
using c2json.Tool.Commands.Extract.Input.Sanitized;

namespace c2json.Tool.Commands.Extract.Domain.Parse;

public sealed class ParseContext : IDisposable
{
    public readonly string FilePath;
    public readonly ExtractTargetPlatformOptions ExtractOptions;
    public readonly ImmutableArray<string> Arguments;
    public readonly ImmutableArray<string> SystemIncludeDirectories;
    public readonly TargetPlatform TargetPlatformRequested;
    public readonly TargetPlatform TargetPlatformActual;
    public readonly int PointerSizeBytes;

    private clang.CXTranslationUnit _translationUnit;

    public ParseContext(
        clang.CXTranslationUnit translationUnit,
        string filePath,
        ExtractTargetPlatformOptions extractOptions,
        ImmutableArray<string> arguments,
        ImmutableArray<string> systemIncludeDirectories)
    {
        _translationUnit = translationUnit;

        FilePath = filePath;
        ExtractOptions = extractOptions;
        Arguments = arguments;
        SystemIncludeDirectories = systemIncludeDirectories;
        TargetPlatformRequested = extractOptions.TargetPlatform;

        var targetInfo = GetTargetInfo(translationUnit);
        TargetPlatformActual = targetInfo.TargetPlatform;
        PointerSizeBytes = targetInfo.PointerSizeBytes;
    }

    public bool IsSystemCursor(clang.CXCursor cursor)
    {
        var cursorLocation = clang.clang_getCursorLocation(cursor);
        var isSystemCursor = clang.clang_Location_isInSystemHeader(cursorLocation) > 0;
        return isSystemCursor;
    }

    public int? SizeOf(CNodeKind nodeKind, clang.CXType type)
    {
        if (nodeKind == CNodeKind.Function)
        {
            return null;
        }

        if (nodeKind == CNodeKind.OpaqueType)
        {
            return 0;
        }

        var sizeOf = (int)clang.clang_Type_getSizeOf(type);
        if (sizeOf >= 0)
        {
            return sizeOf;
        }

        switch (nodeKind)
        {
            case CNodeKind.Pointer:
            case CNodeKind.Array:
                return PointerSizeBytes;
            default:
                return sizeOf;
        }
    }

    public int? AlignOf(CNodeKind kind, clang.CXType containerType)
    {
        if (kind == CNodeKind.OpaqueType)
        {
            return null;
        }

        var alignOfValue = (int)clang.clang_Type_getAlignOf(containerType);
        int? alignOf = alignOfValue >= 0 ? alignOfValue : null;
        return alignOf;
    }

    public bool IsPrimitiveType(clang.CXType type)
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

    public ImmutableArray<clang.CXCursor> GetTranslationUnitExternalTopLevelCursors()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        return translationUnitCursor.GetDescendents(
            static (child, _) => IsExternalTopLevelCursor(child));
    }

    public ImmutableArray<clang.CXCursor> GetTranslationUnitIncludes()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        return translationUnitCursor.GetDescendents(
            static (child, _) => child.kind == clang.CXCursorKind.CXCursor_InclusionDirective);
    }

    public void Dispose()
    {
        TryReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ParseContext()
    {
        TryReleaseUnmanagedResources();
    }

    private void TryReleaseUnmanagedResources()
    {
        unsafe
        {
            if (_translationUnit.Data != null)
            {
                clang.clang_disposeTranslationUnit(_translationUnit);
            }

            _translationUnit.Data = null;
        }
    }

    private (TargetPlatform TargetPlatform, int PointerSizeBytes) GetTargetInfo(
        clang.CXTranslationUnit translationUnit)
    {
        var targetInfo = clang.clang_getTranslationUnitTargetInfo(translationUnit);
        var targetInfoTriple = clang.clang_TargetInfo_getTriple(targetInfo);
        var pointerWidth = clang.clang_TargetInfo_getPointerWidth(targetInfo);
        var pointerSizeBytes = pointerWidth / 8;
        var platformString = targetInfoTriple.String();
        var platform = new TargetPlatform(platformString);
        clang.clang_TargetInfo_dispose(targetInfo);
        return (platform, pointerSizeBytes);
    }

    private static bool IsExternalTopLevelCursor(clang.CXCursor cursor)
    {
        var kind = clang.clang_getCursorKind(cursor);
        if (kind != clang.CXCursorKind.CXCursor_FunctionDecl &&
            kind != clang.CXCursorKind.CXCursor_VarDecl &&
            kind != clang.CXCursorKind.CXCursor_EnumDecl &&
            kind != clang.CXCursorKind.CXCursor_TypedefDecl &&
            kind != clang.CXCursorKind.CXCursor_StructDecl)
        {
            return false;
        }

        if (kind == clang.CXCursorKind.CXCursor_EnumDecl)
        {
            return true;
        }

        var linkage = clang.clang_getCursorLinkage(cursor);
        var visibility = clang.clang_getCursorVisibility(cursor);
        var isExported = linkage == clang.CXLinkageKind.CXLinkage_External &&
                         visibility == clang.CXVisibilityKind.CXVisibility_Default;
        return isExported;
    }
}
