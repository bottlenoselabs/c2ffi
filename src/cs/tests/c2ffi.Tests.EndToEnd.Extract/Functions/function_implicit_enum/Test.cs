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
        function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        returnType.Name.Should().Be("int");
        returnType.NodeKind.Should().Be("primitive");
        returnType.SizeOf.Should().Be(4);
        returnType.AlignOf.Should().Be(4);
        returnType.InnerType.Should().BeNull();

        function.Parameters.Should().HaveCount(1);
        var parameter = function.Parameters[0];
        parameter.Name.Should().Be("value");
        parameter.Type.Name.Should().Be("int");
        parameter.Type.NodeKind.Should().Be("primitive");
        parameter.Type.SizeOf.Should().Be(4);
        parameter.Type.AlignOf.Should().Be(4);
    }

    private void FfiEnumExists(CTestFfiTargetPlatform ffi)
    {
        var @enum = ffi.GetEnum("enum_implicit");
        @enum.Values.Should().HaveCount(2);
        @enum.SizeOf.Should().Be(4);
    }
}