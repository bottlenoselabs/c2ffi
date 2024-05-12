// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;

namespace c2ffi.Tests.Library.Models;

public class CTestType
{
    public string NodeKind { get; }

    public string Name { get; }

    public int? SizeOf { get; }

    public int? AlignOf { get; }

    public bool IsAnonymous { get; set; }

    public CTestType? InnerType { get; }

    public CTestType(CType type)
    {
        NodeKind = type.NodeKind.ToString().ToLowerInvariant();
        Name = type.Name;
        SizeOf = type.SizeOf;
        AlignOf = type.AlignOf;
        IsAnonymous = type.IsAnonymous ?? false;
        InnerType = type.InnerType != null ? new CTestType(type.InnerType) : null;
    }
}
