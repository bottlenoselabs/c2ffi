// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;

namespace c2ffi.Tests.Library.Models;

public class CTestType(CType type)
{
    public string NodeKind { get; } = type.NodeKind.ToString().ToLowerInvariant();

    public string Name { get; } = type.Name;

    public int? SizeOf { get; } = type.SizeOf;

    public int? AlignOf { get; } = type.AlignOf;

    public bool IsAnonymous { get; set; } = type.IsAnonymous ?? false;

    public CTestType? InnerType { get; } = type.InnerType != null ? new CTestType(type.InnerType) : null;
}
