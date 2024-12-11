// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using static bottlenoselabs.clang;

namespace c2ffi.Extract.Explore;

internal sealed class KindTypes
{
    private readonly ImmutableArray<CXTypeKind>? _expectedTypeKinds;

    private KindTypes(ImmutableArray<CXTypeKind>? kinds)
    {
        _expectedTypeKinds = kinds;
    }

    public static KindTypes Is(CXTypeKind kind)
    {
        var typeKinds = ImmutableArray.Create(kind);
        return new KindTypes(typeKinds);
    }

    public static KindTypes Either(params CXTypeKind[] kinds)
    {
        var typeKinds = kinds.ToImmutableArray();
        return new KindTypes(typeKinds);
    }

    public bool Matches(CXTypeKind cursorKind)
    {
        if (_expectedTypeKinds == null)
        {
            return false;
        }

        var expectedCursorKinds = _expectedTypeKinds.Value;
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
    public static readonly KindTypes Any = new(ImmutableArray<CXTypeKind>.Empty);

    public static readonly KindTypes None = new(null);

#pragma warning restore CA2211
}
