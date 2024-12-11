// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;
using static bottlenoselabs.clang;

namespace c2ffi.Extract.Explore;

internal sealed class NodeInfo
{
    public CNodeKind NodeKind { get; init; }

    public string Name { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public CXCursor ClangCursor { get; init; }

    public CXType ClangType { get; init; }

    public CLocation Location { get; init; }

    public int? SizeOf { get; init; }

    public int? AlignOf { get; init; }

    public NodeInfo? Parent { get; init; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }

        if (!string.IsNullOrEmpty(TypeName))
        {
            return TypeName;
        }

        return "???";
    }
}
