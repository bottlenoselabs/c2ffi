// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_ignored;

public class Test : ExtractFfiTest
{
    private readonly string[] _functionNamesThatShouldExist =
    [
        "function_allowed"
    ];

    private readonly string[] _functionNamesThatShouldNotExist =
    [
        "function_not_allowed",
        "function_ignored_1",
        "function_ignored_2"
    ];

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/functions/function_ignored/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FunctionsExist(ffi, _functionNamesThatShouldExist);
            FunctionsDoNotExist(ffi, _functionNamesThatShouldNotExist);
        }
    }

    private void FunctionsExist(CTestFfiTargetPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var function = ffi.TryGetFunction(name);
            _ = function.Should().NotBeNull();
        }
    }

    private void FunctionsDoNotExist(CTestFfiTargetPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var function = ffi.TryGetFunction(name);
            _ = function.Should().BeNull();
        }
    }
}
