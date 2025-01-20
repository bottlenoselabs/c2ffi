// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using bottlenoselabs;
using c2ffi.Clang;
using c2ffi.Data;

namespace c2ffi.Extract.Parse;

public sealed class ParseContext : IDisposable
{
    public readonly string FilePath;
    public readonly InputSanitizedTargetPlatform InputSanitized;
    public readonly ImmutableArray<string> Arguments;
    public readonly ImmutableArray<string> SystemIncludeDirectories;
    public readonly TargetPlatform TargetPlatformRequested;
    public readonly TargetPlatform TargetPlatformActual;
    public readonly int PointerSize;

    private clang.CXTranslationUnit _translationUnit;

    public clang.CXTranslationUnit TranslationUnit => _translationUnit;

    public ParseContext(
        clang.CXTranslationUnit translationUnit,
        string filePath,
        InputSanitizedTargetPlatform inputSanitized,
        ImmutableArray<string> arguments,
        ImmutableArray<string> systemIncludeDirectories)
    {
        _translationUnit = translationUnit;

        FilePath = filePath;
        InputSanitized = inputSanitized;
        Arguments = arguments;
        SystemIncludeDirectories = systemIncludeDirectories;
        TargetPlatformRequested = inputSanitized.TargetPlatform;

        var targetInfo = GetTargetInfo(translationUnit);
        TargetPlatformActual = targetInfo.TargetPlatform;
        PointerSize = targetInfo.PointerSizeBytes;
    }

    public CLocation Location(clang.CXCursor clangCursor, out bool isFromMainFile)
    {
        var locationSource = clang.clang_getCursorLocation(clangCursor);
        isFromMainFile = clang.clang_Location_isFromMainFile(locationSource) > 0;

        clang.CXFile file;
        uint lineNumber;
        uint columnNumber;
        unsafe
        {
            uint offset;
            clang.clang_getFileLocation(locationSource, &file, &lineNumber, &columnNumber, &offset);
        }

        var fileNamePath = clang.clang_getFileName(file).String();
        var fileName = Path.GetFileName(fileNamePath);
        var fullFilePath = string.IsNullOrEmpty(fileNamePath) ? string.Empty : Path.GetFullPath(fileNamePath);

        var isSystem = false;
        var isFromSystemHeader = clang.clang_Location_isInSystemHeader(locationSource) > 0;
        if (isFromSystemHeader)
        {
            isSystem = true;
        }
        else
        {
            if (string.IsNullOrEmpty(fullFilePath))
            {
                isSystem = true;
            }
            else
            {
                foreach (var systemDirectoryPath in SystemIncludeDirectories)
                {
                    if (!fullFilePath.StartsWith(systemDirectoryPath, StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    isSystem = true;
                    break;
                }
            }
        }

        var location = new CLocation
        {
            FileName = fileName,
            FilePath = fullFilePath,
            FullFilePath = fullFilePath,
            LineNumber = (int)lineNumber,
            LineColumn = (int)columnNumber,
            IsSystem = isSystem
        };

        return location;
    }

    public int? SizeOf(CNodeKind nodeKind, clang.CXType clangType)
    {
#pragma warning disable IDE0010
        switch (nodeKind)
#pragma warning restore IDE0010
        {
            case CNodeKind.Function or CNodeKind.OpaqueType:
            case CNodeKind.Primitive when clangType.kind == clang.CXTypeKind.CXType_Void:
                return null;
        }

        var sizeOf = (int)clang.clang_Type_getSizeOf(clangType);
        if (sizeOf >= 0)
        {
            return sizeOf;
        }

#pragma warning disable IDE0072
        return nodeKind switch
#pragma warning restore IDE0072
        {
            CNodeKind.Pointer or CNodeKind.Array => PointerSize,
            _ => sizeOf
        };
    }

    public int? AlignOf(CNodeKind nodeKind, clang.CXType clangType)
    {
#pragma warning disable IDE0010
        switch (nodeKind)
#pragma warning restore IDE0010
        {
            case CNodeKind.Function or CNodeKind.OpaqueType:
            case CNodeKind.Primitive when clangType.kind == clang.CXTypeKind.CXType_Void:
                return null;
        }

        var alignOfValue = (int)clang.clang_Type_getAlignOf(clangType);
        int? alignOf = alignOfValue >= 0 ? alignOfValue : null;

        // NOTE: `uin64_t` and `double` are 32-bit aligned on 32-bit systems even if they take 8 bytes of storage
        //  see: https://github.com/rust-lang/rust/issues/43899
        // NOTE: for purposes of FFI and bindgen, override the value to be 8 bytes
        if (nodeKind == CNodeKind.Primitive)
        {
            var sizeOf = SizeOf(nodeKind, clangType);
            return sizeOf;
        }

        return alignOf;
    }

    public ImmutableArray<clang.CXCursor> GetIncludes()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);

        return translationUnitCursor.GetDescendents(this, IsInclude);

        static bool IsInclude(ParseContext parseContext, clang.CXCursor cursor, clang.CXCursor cursorParent)
        {
            var location = parseContext.Location(cursor, out var isFromMainFile);
            var isLocationOkay = isFromMainFile && !location.IsSystem;
            if (!isLocationOkay)
            {
                return false;
            }

            var isIncludeHeader = cursor.kind == clang.CXCursorKind.CXCursor_InclusionDirective;
            return isIncludeHeader;
        }
    }

    public ImmutableArray<clang.CXCursor> GetExternalFunctions(ParseContext parseContext)
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        var result = translationUnitCursor.GetDescendents(parseContext, IsExternalFunction);
        return result;

        static bool IsExternalFunction(ParseContext parseContext, clang.CXCursor cursor, clang.CXCursor cursorParent)
        {
            var location = parseContext.Location(cursor, out var isFromMainFile);
            var isLocationOkay = isFromMainFile && !location.IsSystem;
            if (!isLocationOkay)
            {
                return false;
            }

            var isFunction = cursor.kind == clang.CXCursorKind.CXCursor_FunctionDecl;
            var isExternal = clang.clang_getCursorLinkage(cursor) == clang.CXLinkageKind.CXLinkage_External;
            var isVisible = clang.clang_getCursorVisibility(cursor) == clang.CXVisibilityKind.CXVisibility_Default;
            var isExternallyVisibleFunction = isFunction && isExternal && isVisible;
            if (!isExternallyVisibleFunction)
            {
                return false;
            }

            var name = cursor.Spelling();
            var isNameAllowed = parseContext.InputSanitized.IsNameAllowed(name);
            return isNameAllowed;
        }
    }

    public ImmutableArray<clang.CXCursor> GetExternalVariables()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        var result = translationUnitCursor.GetDescendents(this, IsExternalVariable);
        return result;

        static bool IsExternalVariable(ParseContext parseContext, clang.CXCursor cursor, clang.CXCursor parentCursor)
        {
            var location = parseContext.Location(cursor, out var isFromMainFile);
            var isLocationOkay = isFromMainFile && !location.IsSystem;
            if (!isLocationOkay)
            {
                return false;
            }

            var isVariable = cursor.kind == clang.CXCursorKind.CXCursor_VarDecl;
            if (!isVariable)
            {
                return false;
            }

            var name = cursor.Spelling();
            var isNameAllowed = parseContext.InputSanitized.IsNameAllowed(name);
            return isNameAllowed;
        }
    }

    public ImmutableArray<clang.CXCursor> GetMacroObjects()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        return translationUnitCursor.GetDescendents(this, IsMacroObject);

        static bool IsMacroObject(ParseContext parseContext, clang.CXCursor cursor, clang.CXCursor parentCursor)
        {
            var location = parseContext.Location(cursor, out var isFromMainFile);
            var isLocationOkay = isFromMainFile && !location.IsSystem;
            if (!isLocationOkay)
            {
                return false;
            }

            var isMacroDefinition = cursor.kind == clang.CXCursorKind.CXCursor_MacroDefinition;
            var isMacroBuiltIn = clang.clang_Cursor_isMacroBuiltin(cursor) > 0;
            var isMacroFunctionLike = clang.clang_Cursor_isMacroFunctionLike(cursor) > 0;
            var isMacroObject = isMacroDefinition && !isMacroBuiltIn && !isMacroFunctionLike;
            if (!isMacroObject)
            {
                return false;
            }

            var name = cursor.Spelling();
            var isNameAllowed = parseContext.InputSanitized.IsNameAllowed(name);
            return isNameAllowed;
        }
    }

    public ImmutableArray<clang.CXCursor> GetEnums()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        return translationUnitCursor.GetDescendents(this, IsEnum);

        static bool IsEnum(ParseContext parseContext, clang.CXCursor cursor, clang.CXCursor parentCursor)
        {
            var location = parseContext.Location(cursor, out var isFromMainFile);
            var isLocationOkay = isFromMainFile && !location.IsSystem;
            if (!isLocationOkay)
            {
                return false;
            }

            var isEnum = cursor.kind == clang.CXCursorKind.CXCursor_EnumDecl;
            if (!isEnum)
            {
                return false;
            }

            var name = cursor.Spelling();
            var isNameAllowed = parseContext.InputSanitized.IsNameAllowed(name);
            return isNameAllowed;
        }
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
}
