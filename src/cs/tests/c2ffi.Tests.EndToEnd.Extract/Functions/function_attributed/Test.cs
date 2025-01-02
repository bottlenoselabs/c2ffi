// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_attributed;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_attributed";

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

    private void FunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        _ = function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        _ = returnType.Name.Should().Be("void *");
        _ = returnType.SizeOf.Should().Be(ffi.PointerSize);
        _ = returnType.AlignOf.Should().Be(ffi.PointerSize);
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
