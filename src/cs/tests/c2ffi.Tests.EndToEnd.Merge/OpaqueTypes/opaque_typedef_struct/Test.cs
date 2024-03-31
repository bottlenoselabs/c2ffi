// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.OpaqueTypes.opaque_typedef_struct;

public class Test : MergeFfisTest
{
    private const string OpaqueTypeName = "opaque_struct_t";

    [Fact]
    public void OpaqueType()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/opaque_types/opaque_typedef_struct/ffi");
        FfiOpaqueTypeExists(ffi);
    }

    private void FfiOpaqueTypeExists(CTestFfiCrossPlatform ffi)
    {
        const string name = $"struct {OpaqueTypeName}";
        var variable = ffi.GetOpaqueType(name);
        Assert.True(variable.Name == name);
    }
}
