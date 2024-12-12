// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_internal;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_internal";

    [Fact]
    public void Function()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/functions/{FunctionName}/ffi");

        Function1DoesNotExist(ffi);
        Function2DoesExist(ffi);
        Function3DoesNotExist(ffi);
        Function4DoesNotExist(ffi);
        Function5DoesExist(ffi);
    }

    private void Function1DoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_1");
        _ = function.Should().BeNull();
    }

    private void Function2DoesExist(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_2");
        _ = function.Should().NotBeNull();
    }

    private void Function3DoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_3");
        _ = function.Should().BeNull();
    }

    private void Function4DoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_4");
        _ = function.Should().BeNull();
    }

    private void Function5DoesExist(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.TryGetFunction("function_internal_5");
        _ = function.Should().NotBeNull();
    }
}
