// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Enums.enum_uint8;

public class Test : ExtractFfiTest
{
    private const string EnumName = "enum_uint8";

    [Fact]
    public void EnumExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/enums/{EnumName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiEnumExists(ffi);
        }
    }

    private void FfiEnumExists(CTestFfiTargetPlatform ffi)
    {
        const string name = $"enum {EnumName}";
        var @enum = ffi.GetEnum(name);
        @enum.IntegerTypeName.Should().BeOneOf("unsigned int", "int");

        @enum.Values[0].Name.Should().Be("ENUM_UINT8_MIN");
        @enum.Values[0].Value.Should().Be(0);

        @enum.Values[1].Name.Should().Be("ENUM_UINT8_MAX");
        @enum.Values[1].Value.Should().Be(255);
    }
}
