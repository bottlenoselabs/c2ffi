// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_internal;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_internal";

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            Function1DoesNotExist(ffi);
            Function2DoesExist(ffi);
            Function3DoesNotExist(ffi);
            Function4DoesNotExist(ffi);
            Function5DoesExist(ffi);
        }
    }

    private void Function1DoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_1");
        _ = function.Should().BeNull();
    }

    private void Function2DoesExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_2");
        _ = function.Should().NotBeNull();
    }

    private void Function3DoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_3");
        _ = function.Should().BeNull();
    }

    private void Function4DoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_4");
        _ = function.Should().BeNull();
    }

    private void Function5DoesExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_5");
        _ = function.Should().NotBeNull();
    }
}
