// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Enums.enum_week_day;

public class Test : ExtractFfiTest
{
    private const string EnumName = "enum_week_day";

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
        @enum.IntegerTypeName.Should().Be("int");

        @enum.Values[0].Name.Should().Be("ENUM_WEEK_DAY_UNKNOWN");
        @enum.Values[0].Value.Should().Be(-1);

        @enum.Values[1].Name.Should().Be("ENUM_WEEK_DAY_MONDAY");
        @enum.Values[1].Value.Should().Be(1);

        @enum.Values[2].Name.Should().Be("ENUM_WEEK_DAY_TUESDAY");
        @enum.Values[2].Value.Should().Be(2);

        @enum.Values[3].Name.Should().Be("ENUM_WEEK_DAY_WEDNESDAY");
        @enum.Values[3].Value.Should().Be(3);

        @enum.Values[4].Name.Should().Be("ENUM_WEEK_DAY_THURSDAY");
        @enum.Values[4].Value.Should().Be(4);

        @enum.Values[5].Name.Should().Be("ENUM_WEEK_DAY_FRIDAY");
        @enum.Values[5].Value.Should().Be(5);

        @enum.Values[6].Name.Should().Be("_ENUM_WEEK_DAY_MAX");
        @enum.Values[6].Value.Should().Be(6);
    }
}
