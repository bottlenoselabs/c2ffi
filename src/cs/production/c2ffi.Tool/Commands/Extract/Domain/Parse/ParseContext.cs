// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;

namespace c2ffi.Tool.Commands.Extract.Domain.Parse;

public sealed class ParseContext : IDisposable
{
    public readonly string FilePath;
    public readonly ExtractTargetPlatformInput ExtractInput;
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
        ExtractTargetPlatformInput extractInput,
        ImmutableArray<string> arguments,
        ImmutableArray<string> systemIncludeDirectories)
    {
        _translationUnit = translationUnit;

        FilePath = filePath;
        ExtractInput = extractInput;
        Arguments = arguments;
        SystemIncludeDirectories = systemIncludeDirectories;
        TargetPlatformRequested = extractInput.TargetPlatform;

        var targetInfo = GetTargetInfo(translationUnit);
        TargetPlatformActual = targetInfo.TargetPlatform;
        PointerSize = targetInfo.PointerSizeBytes;
    }

    public CLocation Location(clang.CXCursor clangCursor)
    {
        var locationSource = clang.clang_getCursorLocation(clangCursor);
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
        switch (nodeKind)
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

        switch (nodeKind)
        {
            case CNodeKind.Pointer:
            case CNodeKind.Array:
                return PointerSize;
            default:
                return sizeOf;
        }
    }

    public int? AlignOf(CNodeKind nodeKind, clang.CXType clangType)
    {
        switch (nodeKind)
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

        return translationUnitCursor.GetDescendents(IsInclude);

        static bool IsInclude(clang.CXCursor child, clang.CXCursor parent)
        {
            var isFromMainFile = child.IsFromMainFile();
            if (!isFromMainFile)
            {
                return false;
            }

            return child.kind == clang.CXCursorKind.CXCursor_InclusionDirective;
        }
    }

    public ImmutableArray<clang.CXCursor> GetExternalFunctions()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        var result = translationUnitCursor.GetDescendents(IsExternalFunction);
        return result;

        static bool IsExternalFunction(clang.CXCursor child, clang.CXCursor parent)
        {
            var isFromMainFile = child.IsFromMainFile();
            if (!isFromMainFile)
            {
                return false;
            }

            var isFunction = child.kind == clang.CXCursorKind.CXCursor_FunctionDecl;
            return isFunction && IsExternal(child);
        }
    }

    public ImmutableArray<clang.CXCursor> GetExternalVariables()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        var result = translationUnitCursor.GetDescendents(IsExternalVariable);
        return result;

        static bool IsExternalVariable(clang.CXCursor child, clang.CXCursor parent)
        {
            var isFromMainFile = child.IsFromMainFile();
            if (!isFromMainFile)
            {
                return false;
            }

            return child.kind == clang.CXCursorKind.CXCursor_VarDecl && IsExternal(child);
        }
    }

    public ImmutableArray<clang.CXCursor> GetMacroObjects()
    {
        var translationUnitCursor = clang.clang_getTranslationUnitCursor(_translationUnit);
        return translationUnitCursor.GetDescendents(
            (child, _) => IsMacroObject(child));

        bool IsMacroObject(clang.CXCursor clangCursor)
        {
            var isFromMainFile = clangCursor.IsFromMainFile();
            if (!isFromMainFile)
            {
                return false;
            }

            if (clangCursor.kind != clang.CXCursorKind.CXCursor_MacroDefinition)
            {
                return false;
            }

            var isMacroBuiltIn = clang.clang_Cursor_isMacroBuiltin(clangCursor) > 0;
            if (isMacroBuiltIn)
            {
                return false;
            }

            var isMacroFunctionLike = clang.clang_Cursor_isMacroFunctionLike(clangCursor) > 0;
            if (isMacroFunctionLike)
            {
                return false;
            }

            var location = Location(clangCursor);
            return !location.IsSystem;
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

    private static bool IsExternal(clang.CXCursor cursor)
    {
        var spelling = cursor.Spelling();
        if (spelling == "SDL_malloc")
        {
            Console.WriteLine();
        }

        var linkage = clang.clang_getCursorLinkage(cursor);
        if (linkage == clang.CXLinkageKind.CXLinkage_Invalid)
        {
            return false;
        }

        var visibility = clang.clang_getCursorVisibility(cursor);
        if (visibility == clang.CXVisibilityKind.CXVisibility_Invalid)
        {
            return false;
        }

        var isExternalLinkage = linkage == clang.CXLinkageKind.CXLinkage_External;
        var isVisible = visibility == clang.CXVisibilityKind.CXVisibility_Default;
        return isExternalLinkage && isVisible;
    }
}
