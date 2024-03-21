// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using c2json.Data;
using static bottlenoselabs.clang;

#pragma warning disable CA1806

namespace c2json.Tool.Commands.Extract.Infrastructure.Clang
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

        public static CLocation Location(this CXCursor clangCursor)
        {
            var locationSource = clang_getCursorLocation(clangCursor);
            CXFile file;
            uint lineNumber;
            uint columnNumber;
            uint offset;
            unsafe
            {
                clang_getFileLocation(locationSource, &file, &lineNumber, &columnNumber, &offset);
            }

            var fileNamePath = clang_getFileName(file).String();
            var fileName = Path.GetFileName(fileNamePath);
            var fullFilePath = string.IsNullOrEmpty(fileNamePath) ? string.Empty : Path.GetFullPath(fileNamePath);

            var location = new CLocation
            {
                FileName = fileName,
                FilePath = fullFilePath,
                FullFilePath = fullFilePath,
                LineNumber = (int)lineNumber,
                LineColumn = (int)columnNumber
            };

            return location;
        }

        public static string GetCode(
            this CXCursor cursor,
            StringBuilder? stringBuilder = null)
        {
            if (stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            else
            {
                stringBuilder.Clear();
            }

            var translationUnit = clang_Cursor_getTranslationUnit(cursor);
            var cursorExtent = clang_getCursorExtent(cursor);
            unsafe
            {
                var tokens = (CXToken*)0;
                uint tokensCount = 0;
                clang_tokenize(translationUnit, cursorExtent, &tokens, &tokensCount);
                for (var i = 0; i < tokensCount; i++)
                {
                    var tokenString = clang_getTokenSpelling(translationUnit, tokens[i]).String();
                    stringBuilder.Append(tokenString);
                }

                clang_disposeTokens(translationUnit, tokens, tokensCount);
            }

            var result = stringBuilder.ToString();
            stringBuilder.Clear();
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
