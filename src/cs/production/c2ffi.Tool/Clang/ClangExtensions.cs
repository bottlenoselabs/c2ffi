// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using c2ffi.Extract.Parse;
using static bottlenoselabs.clang;

#pragma warning disable CA1806

namespace c2ffi.Clang
{
    internal static class ClangExtensions
    {
        internal delegate bool VisitChildPredicate(
            ParseContext parseContext, CXCursor child, CXCursor parent);

        private static VisitChildInstance[] _visitChildInstances = new VisitChildInstance[512];
        private static int _visitChildCount;
        private static VisitFieldsInstance[] _visitFieldsInstances = new VisitFieldsInstance[512];
        private static int _visitFieldsCount;

        private static readonly CXCursorVisitor VisitorChild;
        private static readonly CXFieldVisitor VisitorField;
        private static readonly VisitChildPredicate EmptyVisitChildPredicate = static (_, _, _) => true;

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
#pragma warning disable IDE0072
            return clangType.kind switch
#pragma warning restore IDE0072
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
#pragma warning disable IDE0072
            return clangType.kind switch
#pragma warning restore IDE0072
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

        public static ImmutableArray<CXCursor> GetDescendents(
            this CXCursor cursor,
            ParseContext parseContext,
            VisitChildPredicate? predicate = null,
            bool isRecurse = false)
        {
            var predicate2 = predicate ?? EmptyVisitChildPredicate;
            var visitData = new VisitChildInstance(parseContext, predicate2, isRecurse);
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

            _ = clang_visitChildren(cursor, VisitorChild, clientData);

            _ = Interlocked.Decrement(ref _visitChildCount);
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

            _ = clang_Type_visitFields(type, VisitorField, clientData);

            _ = Interlocked.Decrement(ref _visitFieldsCount);
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
            var result = data.Predicate(data.ParseContext, child, parent);

            if (!result)
            {
                return data.IsRecurse
                    ? CXChildVisitResult.CXChildVisit_Recurse
                    : CXChildVisitResult.CXChildVisit_Continue;
            }

            data.CursorBuilder.Add(child);

            return data.IsRecurse
                ? CXChildVisitResult.CXChildVisit_Break
                : CXChildVisitResult.CXChildVisit_Continue;
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

        private readonly struct VisitChildInstance(
            ParseContext parseContext, VisitChildPredicate predicate, bool isRecurse)
        {
            public readonly bool IsRecurse = isRecurse;
            public readonly ParseContext ParseContext = parseContext;
            public readonly VisitChildPredicate Predicate = predicate;
            public readonly ImmutableArray<CXCursor>.Builder CursorBuilder = ImmutableArray.CreateBuilder<CXCursor>();
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
