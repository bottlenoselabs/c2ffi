// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Functions.function_ignored;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_allowed";
    private const string IgnoredFunctionName = "function_not_allowed";

    [Fact]
    public void Function()
    {
        var ffi = GetCrossPlatformFfi(
            "src/c/tests/functions/function_ignored/ffi");

        FfiFunctionExists(ffi);
        FfiFunctionDoesNotExist(ffi);
    }

    private void FfiFunctionExists(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        function.CallingConvention.Should().Be("cdecl");

        var returnType = function.ReturnType;
        returnType.Name.Should().Be("void");
        returnType.NodeKind.Should().Be("primitive");
        returnType.SizeOf.Should().BeNull();
        returnType.AlignOf.Should().BeNull();
        returnType.InnerType.Should().BeNull();

        function.Parameters.Should().BeEmpty();
    }

    private void FfiFunctionDoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var function = ffi.TryGetFunction(IgnoredFunctionName);
        function.Should().Be(null);
    }
}
