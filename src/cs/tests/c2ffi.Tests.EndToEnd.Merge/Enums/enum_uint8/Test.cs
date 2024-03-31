// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Enums.enum_uint8;

public class Test : MergeFfisTest
{
    private const string EnumName = "enum_uint8";

    [Fact]
    public void EnumExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/enums/{EnumName}/ffi");
        FfiEnumExists(ffi);
    }

    private void FfiEnumExists(CTestFfiCrossPlatform ffi)
    {
        const string name = $"enum {EnumName}";
        var @enum = ffi.GetEnum(name);
        @enum.SizeOf.Should().Be(4);

        @enum.Values[0].Name.Should().Be("ENUM_UINT8_MIN");
        @enum.Values[0].Value.Should().Be(0);

        @enum.Values[1].Name.Should().Be("ENUM_UINT8_MAX");
        @enum.Values[1].Value.Should().Be(255);
    }
}
