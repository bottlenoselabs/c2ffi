// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data;

namespace c2ffi.Tests.Library.Models;

public class CTestTypeInfo
{
    public string Name { get; }

    public int? SizeOf { get; }

    public int? AlignOf { get; }

    public CTestTypeInfo? InnerType { get; }

    public CTestTypeInfo(CTypeInfo typeInfo)
    {
        Name = typeInfo.Name;
        SizeOf = typeInfo.SizeOf;
        AlignOf = typeInfo.AlignOf;
        InnerType = typeInfo.InnerTypeInfo != null ? new CTestTypeInfo(typeInfo.InnerTypeInfo) : null;
    }
}
