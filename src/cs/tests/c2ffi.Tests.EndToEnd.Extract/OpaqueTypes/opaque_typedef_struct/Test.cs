// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.OpaqueTypes.opaque_typedef_struct;

public class Test : ExtractFfiTest
{
    private const string OpaqueTypeName = "opaque_struct_t";

    [Fact]
    public void OpaqueType()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/opaque_types/opaque_typedef_struct/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiOpaqueTypeExists(ffi);
        }
    }

    private void FfiOpaqueTypeExists(CTestFfiTargetPlatform ffi)
    {
        const string name = $"struct {OpaqueTypeName}";
        var variable = ffi.GetOpaqueType(name);
        Assert.True(variable.Name == name);
    }
}
