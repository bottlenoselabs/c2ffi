// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Enums.enum_dangling;

public class Test : MergeFfisTest
{
    private const string EnumName = "enum_dangling";

    [Fact]
    public void Function()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/enums/{EnumName}/ffi");

        EnumExists(ffi);
    }

    private void EnumExists(CTestFfiCrossPlatform ffi)
    {
        var @enum = ffi.GetEnum("enum_dangling");
        _ = @enum.Values.Should().HaveCount(2);
        _ = @enum.SizeOf.Should().Be(4);
    }
}
