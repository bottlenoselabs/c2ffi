// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_int_params_int;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_int_params_int";

    [Fact]
    public void FunctionExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/functions/{FunctionName}/ffi");

        FfiFunctionExists(ffi);
    }

    private void FfiFunctionExists(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        function.CallingConvention.Should().Be("cdecl");
        function.ReturnTypeName.Should().Be("int");
        function.ReturnTypeSizeOf.Should().Be(4);

        function.Parameters.Length.Should().Be(1);
        var parameter = function.Parameters[0];
        parameter.Name.Should().Be("a");
        parameter.TypeName.Should().Be("int");
    }
}
