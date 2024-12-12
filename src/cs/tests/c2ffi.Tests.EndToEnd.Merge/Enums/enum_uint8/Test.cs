// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Enums.enum_uint8;

public class Test : MergeFfisTest
{
    private const string EnumName = "enum_uint8";

    [Fact]
    public void Enum()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/enums/{EnumName}/ffi");
        FfiEnumExists(ffi);
    }

    private void FfiEnumExists(CTestFfiCrossPlatform ffi)
    {
        const string name = $"enum {EnumName}";
        var @enum = ffi.GetEnum(name);
        _ = @enum.SizeOf.Should().Be(4);

        _ = @enum.Values[0].Name.Should().Be("ENUM_UINT8_MIN");
        _ = @enum.Values[0].Value.Should().Be(0);

        _ = @enum.Values[1].Name.Should().Be("ENUM_UINT8_MAX");
        _ = @enum.Values[1].Value.Should().Be(255);
    }
}
