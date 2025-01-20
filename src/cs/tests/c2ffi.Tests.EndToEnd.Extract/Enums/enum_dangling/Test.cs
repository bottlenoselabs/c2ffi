// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Enums.enum_dangling;

public class Test : ExtractFfiTest
{
    private const string EnumName = "enum_dangling";

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/enums/{EnumName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            EnumExists(ffi);
        }
    }

    private void EnumExists(CTestFfiTargetPlatform ffi)
    {
        var @enum = ffi.GetEnum("enum_dangling");
        _ = @enum.Values.Should().HaveCount(2);
        _ = @enum.SizeOf.Should().Be(4);
    }
}
