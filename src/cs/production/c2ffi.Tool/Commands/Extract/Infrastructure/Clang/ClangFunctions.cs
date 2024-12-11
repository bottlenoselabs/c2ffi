// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Infrastructure.Clang;

internal static unsafe class ClangFunctions
{
    private static VisitChildInstance[] _visitChildInstances = new VisitChildInstance[512];
    private static int _visitChildCount;
    private static VisitFieldsInstance[] _visitFieldsInstances = new VisitFieldsInstance[512];
    private static int _visitFieldsCount;

    private static readonly CXCursorVisitor VisitorChild;
    private static readonly CXCursorVisitor VisitorAttribute;
    private static readonly CXFieldVisitor VisitorField;

    private static readonly ClangVisitCursorChildPredicate EmptyVisitCursorChildPredicate = static (_, _) => true;

    static ClangFunctions()
    {
        VisitorChild.Pointer = &VisitChild;
        VisitorAttribute.Pointer = &VisitAttribute;
        VisitorField.Pointer = &VisitField;
    }

    public static ImmutableArray<CXCursor> VisitChildren(
        CXCursor cursor,
        ClangVisitCursorChildPredicate? predicate = null)
    {
        var predicate2 = predicate ?? EmptyVisitCursorChildPredicate;
        var visitData = new VisitChildInstance(predicate2);
        var visitsCount = Interlocked.Increment(ref _visitChildCount);
        if (visitsCount > _visitChildInstances.Length)
        {
            Array.Resize(ref _visitChildInstances, visitsCount * 2);
        }

        _visitChildInstances[visitsCount - 1] = visitData;

        var clientData = default(CXClientData);
        clientData.Data = (void*)_visitChildCount;
        _ = clang_visitChildren(cursor, VisitorChild, clientData) > 0;

        _ = Interlocked.Decrement(ref _visitChildCount);
        var result = visitData.CursorBuilder.ToImmutable();
        visitData.CursorBuilder.Clear();
        return result;
    }

    public static ImmutableArray<CXCursor> VisitChildrenAttributes(
        CXCursor cursor,
        ClangVisitCursorChildPredicate? predicate = null)
    {
        var hasAttributes = clang_Cursor_hasAttrs(cursor) > 0;
        if (!hasAttributes)
        {
            return ImmutableArray<CXCursor>.Empty;
        }

        var predicate2 = predicate ?? EmptyVisitCursorChildPredicate;
        var visitData = new VisitChildInstance(predicate2);
        var visitsCount = Interlocked.Increment(ref _visitChildCount);
        if (visitsCount > _visitChildInstances.Length)
        {
            Array.Resize(ref _visitChildInstances, visitsCount * 2);
        }

        _visitChildInstances[visitsCount - 1] = visitData;

        var clientData = default(CXClientData);
        clientData.Data = (void*)_visitChildCount;
        _ = clang_visitChildren(cursor, VisitorAttribute, clientData);

        _ = Interlocked.Decrement(ref _visitChildCount);
        var result = visitData.CursorBuilder.ToImmutable();
        visitData.CursorBuilder.Clear();
        return result;
    }

    public static ImmutableArray<CXCursor> VisitFields(CXType type)
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
        clientData.Data = (void*)_visitFieldsCount;
        _ = clang_Type_visitFields(type, VisitorField, clientData);

        _ = Interlocked.Decrement(ref _visitFieldsCount);
        var result = visitData.CursorBuilder.ToImmutable();
        visitData.CursorBuilder.Clear();
        return result;
    }

    [UnmanagedCallersOnly]
    private static CXChildVisitResult VisitChild(CXCursor child, CXCursor parent, CXClientData clientData)
    {
        var index = (int)clientData.Data;
        var data = ClangFunctions._visitChildInstances[index - 1];

        var result = data.Predicate(child, parent);
        if (!result)
        {
            return CXChildVisitResult.CXChildVisit_Continue;
        }

        data.CursorBuilder.Add(child);

        return CXChildVisitResult.CXChildVisit_Continue;
    }

    [UnmanagedCallersOnly]
    private static CXChildVisitResult VisitAttribute(CXCursor child, CXCursor parent, CXClientData clientData)
    {
        var index = (int)clientData.Data;
        var data = _visitChildInstances[index - 1];

        /*var isAttribute = clang_isAttribute(child.kind) > 0;
        if (!isAttribute)
        {
            return CXChildVisitResult.CXChildVisit_Continue;
        }*/

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
        var index = (int)clientData.Data;
        var data = _visitFieldsInstances[index - 1];
        data.CursorBuilder.Add(cursor);
        return CXVisitorResult.CXVisit_Continue;
    }

    private readonly struct VisitChildInstance(ClangVisitCursorChildPredicate predicate)
    {
        public readonly ClangVisitCursorChildPredicate Predicate = predicate;
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
