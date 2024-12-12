// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_void;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_void";

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
        _ = returnType.Name.Should().Be("void");
        _ = returnType.NodeKind.Should().Be("primitive");
        _ = returnType.SizeOf.Should().BeNull();
        _ = returnType.AlignOf.Should().BeNull();
        _ = returnType.InnerType.Should().BeNull();

        _ = function.Parameters.Should().BeEmpty();
    }
}
