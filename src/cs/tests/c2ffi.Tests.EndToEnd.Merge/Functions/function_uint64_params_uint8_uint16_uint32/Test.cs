// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_uint64_params_uint8_uint16_uint32;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_uint64_params_uint8_uint16_uint32";

    [Fact]
    public void Function()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/functions/{FunctionName}/ffi");

        FfiFunctionExists(ffi);
    }

    private static void FfiFunctionExists(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        returnType.Name.Should().Be("uint64_t");
        returnType.NodeKind.Should().Be("typealias");
        returnType.SizeOf.Should().Be(8);
        returnType.AlignOf.Should().Be(8);
        returnType.InnerType.Should().NotBeNull();
        returnType.InnerType!.NodeKind.Should().Be("primitive");
        returnType.InnerType!.InnerType.Should().BeNull();

        var returnTypeInner = returnType.InnerType!;
        returnTypeInner.Should().NotBeNull();
        returnTypeInner.SizeOf.Should().Be(returnType.SizeOf);
        returnTypeInner.AlignOf.Should().Be(returnType.AlignOf);

        function.Parameters.Length.Should().Be(3);

        var parameter1 = function.Parameters[0];
        parameter1.Name.Should().Be("a");
        parameter1.TypeName.Should().Be("uint8_t");

        var parameter2 = function.Parameters[1];
        parameter2.Name.Should().Be("b");
        parameter2.TypeName.Should().Be("uint16_t");

        var parameter3 = function.Parameters[2];
        parameter3.Name.Should().Be("c");
        parameter3.TypeName.Should().Be("uint32_t");
    }
}
