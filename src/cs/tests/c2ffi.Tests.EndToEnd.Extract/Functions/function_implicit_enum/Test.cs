// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_implicit_enum;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_implicit_enum";

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiFunctionExists(ffi);
            FfiEnumExists(ffi);
        }
    }

    private void FfiFunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        _ = function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        _ = returnType.Name.Should().Be("int");
        _ = returnType.NodeKind.Should().Be("primitive");
        _ = returnType.SizeOf.Should().Be(4);
        _ = returnType.AlignOf.Should().Be(4);
        _ = returnType.InnerType.Should().BeNull();

        _ = function.Parameters.Should().HaveCount(1);
        var parameter = function.Parameters[0];
        _ = parameter.Name.Should().Be("value");
        _ = parameter.Type.Name.Should().Be("int");
        _ = parameter.Type.NodeKind.Should().Be("primitive");
        _ = parameter.Type.SizeOf.Should().Be(4);
        _ = parameter.Type.AlignOf.Should().Be(4);
    }

    private void FfiEnumExists(CTestFfiTargetPlatform ffi)
    {
        var @enum = ffi.GetEnum("enum_implicit");
        _ = @enum.Values.Should().HaveCount(2);
        _ = @enum.SizeOf.Should().Be(4);
    }
}
