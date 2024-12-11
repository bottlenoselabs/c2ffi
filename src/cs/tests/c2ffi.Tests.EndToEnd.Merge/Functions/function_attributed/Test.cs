// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_attributed;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_attributed";

    [Fact]
    public void Function()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/functions/{FunctionName}/ffi");
        FfiFunctionExists(ffi);
    }

    private void FfiFunctionExists(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        _ = function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        _ = returnType.Name.Should().Be("void *");
        _ = returnType.NodeKind.Should().Be("pointer");
        _ = returnType.InnerType.Should().NotBeNull();
        _ = returnType.InnerType!.Name.Should().Be("void");
        _ = returnType.InnerType!.NodeKind.Should().Be("primitive");
        _ = returnType.InnerType.InnerType.Should().BeNull();

        _ = function.Parameters.Should().NotBeEmpty();
        _ = function.Parameters.Length.Should().Be(1);
        var parameter = function.Parameters[0];
        _ = parameter.Name.Should().Be("size");
        _ = parameter.Type.Name.Should().Be("size_t");
        _ = parameter.Type.InnerType.Should().NotBeNull();
    }
}
