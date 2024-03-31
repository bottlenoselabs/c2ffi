// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.OpaqueTypes.opaque_struct;

public class Test : ExtractFfiTest
{
    private const string OpaqueTypeName = "opaque_struct";

    [Fact]
    public void OpaqueTypeExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/opaque_types/{OpaqueTypeName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiOpaqueTypeExists(ffi);
        }
    }

    private void FfiOpaqueTypeExists(CTestFfiTargetPlatform ffi)
    {
        var variable = ffi.GetOpaqueType(OpaqueTypeName);
        Assert.True(variable.Name == OpaqueTypeName);
    }
}
