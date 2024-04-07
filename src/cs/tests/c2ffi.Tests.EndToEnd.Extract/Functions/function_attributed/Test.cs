// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

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
            FfiFunctionExists(ffi);
        }
    }

    private void FfiFunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        returnType.Name.Should().Be("void *");
        returnType.SizeOf.Should().Be(ffi.PointerSize);
        returnType.AlignOf.Should().Be(ffi.PointerSize);
        returnType.NodeKind.Should().Be("pointer");
        returnType.InnerType.Should().NotBeNull();
        returnType.InnerType!.Name.Should().Be("void");
        returnType.InnerType!.NodeKind.Should().Be("primitive");
        returnType.InnerType.InnerType.Should().BeNull();

        function.Parameters.Should().NotBeEmpty();
        function.Parameters.Length.Should().Be(1);
        var parameter = function.Parameters[0];
        parameter.Name.Should().Be("size");
        parameter.Type.Name.Should().Be("size_t");
        parameter.Type.InnerType.Should().NotBeNull();
    }
}
