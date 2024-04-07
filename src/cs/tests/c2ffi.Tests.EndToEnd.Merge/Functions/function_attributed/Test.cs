// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

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
        function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        returnType.Name.Should().Be("void *");
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
