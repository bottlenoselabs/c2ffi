// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.OpaqueTypes.opaque_struct;

public class Test : MergeFfisTest
{
    private const string OpaqueTypeName = "opaque_struct";

    [Fact]
    public void OpaqueTypeExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/opaque_types/{OpaqueTypeName}/ffi");
        FfiOpaqueTypeExists(ffi);
    }

    private void FfiOpaqueTypeExists(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.GetOpaqueType(OpaqueTypeName);
        Assert.True(variable.Name == OpaqueTypeName);
    }
}
