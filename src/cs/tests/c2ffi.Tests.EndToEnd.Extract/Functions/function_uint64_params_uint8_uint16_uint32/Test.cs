// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_uint64_params_uint8_uint16_uint32;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_uint64_params_uint8_uint16_uint32";

    [Fact]
    public void FunctionExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiFunctionExists(ffi);
        }
    }

    private static void FfiFunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        function.CallingConvention.Should().Be("cdecl");
        function.ReturnTypeName.Should().Be("uint64_t");
        function.ReturnTypeSizeOf.Should().Be(8);

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
