// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_ignored;

public class Test : MergeFfisTest
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
        var ffi = GetCrossPlatformFfi("src/c/tests/functions/function_ignored/ffi");

        FunctionsExist(ffi, _functionNamesThatShouldExist);
        FunctionsDoNotExist(ffi, _functionNamesThatShouldNotExist);
    }

    private void FunctionsExist(CTestFfiCrossPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var function = ffi.TryGetFunction(name);
            _ = function.Should().NotBeNull();
        }
    }

    private void FunctionsDoNotExist(CTestFfiCrossPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var function = ffi.TryGetFunction(name);
            _ = function.Should().BeNull();
        }
    }
}
