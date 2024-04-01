// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using c2ffi.Data;
using static bottlenoselabs.clang;

#pragma warning disable CA1806

namespace c2ffi.Tool.Commands.Extract.Infrastructure.Clang
{
    public static class ClangExtensions
    {
        public delegate bool VisitChildPredicate(CXCursor child, CXCursor parent);

        private static VisitChildInstance[] _visitChildInstances = new VisitChildInstance[512];
        private static int _visitChildCount;
        private static VisitFieldsInstance[] _visitFieldsInstances = new VisitFieldsInstance[512];
        private static int _visitFieldsCount;

        private static readonly CXCursorVisitor VisitorChild;
        private static readonly CXFieldVisitor VisitorField;
        private static readonly VisitChildPredicate EmptyVisitChildPredicate = static (_, _) => true;

        static ClangExtensions()
        {
            unsafe
            {
                VisitorChild.Pointer = &VisitChild;
                VisitorField.Pointer = &VisitField;
            }
        }

        public static string String(this CXString cxString)
        {
            var cString = clang_getCString(cxString);
            var result = Marshal.PtrToStringAnsi(cString);
            clang_disposeString(cxString);
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }

        public static string Spelling(this CXCursor clangCursor)
        {
            return clang_getCursorSpelling(clangCursor).String();
        }

        public static string Spelling(this CXType clangType)
        {
            return clang_getTypeSpelling(clangType).String();
        }

        public static bool IsPrimitive(this CXType clangType)
        {
            return clangType.kind switch
            {
                CXTypeKind.CXType_Void => true,
                CXTypeKind.CXType_Bool => true,
                CXTypeKind.CXType_Char_S => true,
                CXTypeKind.CXType_SChar => true,
                CXTypeKind.CXType_Char_U => true,
                CXTypeKind.CXType_UChar => true,
                CXTypeKind.CXType_UShort => true,
                CXTypeKind.CXType_UInt => true,
                CXTypeKind.CXType_ULong => true,
                CXTypeKind.CXType_ULongLong => true,
                CXTypeKind.CXType_Short => true,
                CXTypeKind.CXType_Int => true,
                CXTypeKind.CXType_Long => true,
                CXTypeKind.CXType_LongLong => true,
                CXTypeKind.CXType_Float => true,
                CXTypeKind.CXType_Double => true,
                CXTypeKind.CXType_LongDouble => true,
                _ => false
            };
        }

        public static bool IsSignedPrimitive(this CXType clangType)
        {
            return clangType.kind switch
            {
                CXTypeKind.CXType_Char_S => true,
                CXTypeKind.CXType_SChar => true,
                CXTypeKind.CXType_Char_U => true,
                CXTypeKind.CXType_Short => true,
                CXTypeKind.CXType_Int => true,
                CXTypeKind.CXType_Long => true,
                CXTypeKind.CXType_LongLong => true,
                _ => false
            };
        }

        public static unsafe bool TryParseTranslationUnit(
            string filePath,
            ImmutableArray<string> commandLineArgs,
            out CXTranslationUnit translationUnit,
            bool skipFunctionBodies = true,
            bool keepGoing = false)
        {
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            uint options = 0x0 |
                           0x1 | // CXTranslationUnit_DetailedPreprocessingRecord
                           0x80 | // IncludeBriefCommentsInCodeCompletion
                           0x1000 | // CXTranslationUnit_IncludeAttributedTypes
                           0x2000 | // CXTranslationUnit_VisitImplicitAttributes
                           0x4000 | // CXTranslationUnit_IgnoreNonErrorsFromIncludedFiles
                           0x0;

            if (skipFunctionBodies)
            {
                options |= 0x40; // CXTranslationUnit_SkipFunctionBodies
            }

            if (keepGoing)
            {
                options |= 0x200; // CXTranslationUnit_KeepGoing
            }

            var index = clang_createIndex(0, 0);
            var cSourceFilePath = CString.FromString(filePath);
            var cCommandLineArgs = CStrings.CStringArray(commandLineArgs.AsSpan());

            CXErrorCode errorCode;
            fixed (CXTranslationUnit* translationUnitPointer = &translationUnit)
            {
                errorCode = clang_parseTranslationUnit2(
                    index,
                    cSourceFilePath,
                    cCommandLineArgs,
                    commandLineArgs.Length,
                    (CXUnsavedFile*)IntPtr.Zero,
                    0,
                    options,
                    translationUnitPointer);
            }

            var result = errorCode == CXErrorCode.CXError_Success;
            return result;
        }

        public static ImmutableArray<CXCursor> GetDescendents(
            this CXCursor cursor, VisitChildPredicate? predicate = null, bool mustBeFromSameFile = true)
        {
            var predicate2 = predicate ?? EmptyVisitChildPredicate;
            var visitData = new VisitChildInstance(predicate2, mustBeFromSameFile);
            var visitsCount = Interlocked.Increment(ref _visitChildCount);
            if (visitsCount > _visitChildInstances.Length)
            {
                Array.Resize(ref _visitChildInstances, visitsCount * 2);
            }

            _visitChildInstances[visitsCount - 1] = visitData;

            var clientData = default(CXClientData);
            unsafe
            {
                clientData.Data = (void*)_visitChildCount;
            }

            clang_visitChildren(cursor, VisitorChild, clientData);

            Interlocked.Decrement(ref _visitChildCount);
            var result = visitData.CursorBuilder.ToImmutable();
            visitData.CursorBuilder.Clear();
            return result;
        }

        public static ImmutableArray<CXCursor> GetFields(this CXType type)
        {
#pragma warning disable SA1129
            var visitData = new VisitFieldsInstance();
#pragma warning restore SA1129
            var visitsCount = Interlocked.Increment(ref _visitFieldsCount);
            if (visitsCount > _visitFieldsInstances.Length)
            {
                Array.Resize(ref _visitFieldsInstances, visitsCount * 2);
            }

            _visitFieldsInstances[visitsCount - 1] = visitData;

            var clientData = default(CXClientData);
            unsafe
            {
                clientData.Data = (void*)_visitFieldsCount;
            }

            clang_Type_visitFields(type, VisitorField, clientData);

            Interlocked.Decrement(ref _visitFieldsCount);
            var result = visitData.CursorBuilder.ToImmutable();
            visitData.CursorBuilder.Clear();
            return result;
        }

        [UnmanagedCallersOnly]
        private static CXChildVisitResult VisitChild(CXCursor child, CXCursor parent, CXClientData clientData)
        {
            int index;
            unsafe
            {
                index = (int)clientData.Data;
            }

            var data = _visitChildInstances[index - 1];

            if (data.MustBeFromSameFile)
            {
                var location = clang_getCursorLocation(child);
                var isFromMainFile = clang_Location_isFromMainFile(location) > 0;
                if (!isFromMainFile)
                {
                    return CXChildVisitResult.CXChildVisit_Continue;
                }
            }

            var result = data.Predicate(child, parent);
            if (!result)
            {
                return CXChildVisitResult.CXChildVisit_Continue;
            }

            data.CursorBuilder.Add(child);

            return CXChildVisitResult.CXChildVisit_Continue;
        }

        [UnmanagedCallersOnly]
        private static CXVisitorResult VisitField(CXCursor cursor, CXClientData clientData)
        {
            int index;
            unsafe
            {
                index = (int)clientData.Data;
            }

            var data = _visitFieldsInstances[index - 1];
            data.CursorBuilder.Add(cursor);
            return CXVisitorResult.CXVisit_Continue;
        }

        private readonly struct VisitChildInstance
        {
            public readonly VisitChildPredicate Predicate;
            public readonly ImmutableArray<CXCursor>.Builder CursorBuilder;
            public readonly bool MustBeFromSameFile;

            public VisitChildInstance(VisitChildPredicate predicate, bool mustBeFromSameFile)
            {
                Predicate = predicate;
                CursorBuilder = ImmutableArray.CreateBuilder<CXCursor>();
                MustBeFromSameFile = mustBeFromSameFile;
            }
        }

        private readonly struct VisitFieldsInstance
        {
            public readonly ImmutableArray<CXCursor>.Builder CursorBuilder;

            public VisitFieldsInstance()
            {
                CursorBuilder = ImmutableArray.CreateBuilder<CXCursor>();
            }
        }
    }
}
