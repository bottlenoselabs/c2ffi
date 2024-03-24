// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore;

public sealed class ExploreCandidateInfoNode
{
    public CNodeKind NodeKind { get; init; }

    public string Name { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public CXCursor Cursor { get; init; }

    public CXType Type { get; init; }

    public CLocation? Location { get; init; }

    public int? SizeOf { get; init; }

    public int? AlignOf { get; init; }

    public bool IsAnonymous { get; init; }

    public ExploreCandidateInfoNode? Parent { get; init; }

    public override string ToString()
    {
        return Name;
    }
}
