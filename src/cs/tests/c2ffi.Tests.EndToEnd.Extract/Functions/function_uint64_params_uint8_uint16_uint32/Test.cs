// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_uint64_params_uint8_uint16_uint32;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_uint64_params_uint8_uint16_uint32";

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FunctionExists(ffi);
        }
    }

    private static void FunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        _ = function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        _ = returnType.Name.Should().Be("uint64_t");
        _ = returnType.NodeKind.Should().Be("typealias");
        _ = returnType.SizeOf.Should().Be(8);
        _ = returnType.AlignOf.Should().Be(8);
        _ = returnType.InnerType.Should().NotBeNull();

        _ = function.Parameters.Length.Should().Be(3);

        var parameter1 = function.Parameters[0];
        _ = parameter1.Name.Should().Be("a");
        _ = parameter1.Type.Name.Should().Be("uint8_t");

        var parameter2 = function.Parameters[1];
        _ = parameter2.Name.Should().Be("b");
        _ = parameter2.Type.Name.Should().Be("uint16_t");

        var parameter3 = function.Parameters[2];
        _ = parameter3.Name.Should().Be("c");
        _ = parameter3.Type.Name.Should().Be("uint32_t");
    }
}
