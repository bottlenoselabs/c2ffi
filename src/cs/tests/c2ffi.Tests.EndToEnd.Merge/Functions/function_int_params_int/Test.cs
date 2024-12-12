// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_int_params_int;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_int_params_int";

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
        _ = returnType.Name.Should().Be("int");
        _ = returnType.NodeKind.Should().Be("primitive");
        _ = returnType.SizeOf.Should().Be(4);
        _ = returnType.AlignOf.Should().Be(4);
        _ = returnType.InnerType.Should().BeNull();

        _ = function.Parameters.Length.Should().Be(1);
        var parameter = function.Parameters[0];
        _ = parameter.Name.Should().Be("a");
        _ = parameter.Type.Name.Should().Be("int");
    }
}
