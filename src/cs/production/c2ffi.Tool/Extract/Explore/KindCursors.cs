// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using static bottlenoselabs.clang;

namespace c2ffi.Extract.Explore;

internal sealed class KindCursors
{
    private readonly ImmutableArray<CXCursorKind>? _expectedCursorKinds;

    private KindCursors(ImmutableArray<CXCursorKind>? kinds)
    {
        _expectedCursorKinds = kinds;
    }

    public static KindCursors Is(CXCursorKind kind)
    {
        var cursorKinds = ImmutableArray.Create(kind);
        return new KindCursors(cursorKinds);
    }

    public static KindCursors Either(params CXCursorKind[] kinds)
    {
        var cursorKinds = kinds.ToImmutableArray();
        return new KindCursors(cursorKinds);
    }

    public bool Matches(CXCursorKind cursorKind)
    {
        if (_expectedCursorKinds == null)
        {
            return false;
        }

        var expectedCursorKinds = _expectedCursorKinds.Value;
        if (expectedCursorKinds.IsDefaultOrEmpty)
        {
            return true;
        }

        var isAsExpected = false;
        foreach (var expectedCursorKind in expectedCursorKinds)
        {
            if (cursorKind != expectedCursorKind)
            {
                continue;
            }

            isAsExpected = true;
            break;
        }

        return isAsExpected;
    }

#pragma warning disable CA2211
    public static readonly KindCursors Any = new(ImmutableArray<CXCursorKind>.Empty);

    public static readonly KindCursors None = new(null);

#pragma warning restore CA2211
}
