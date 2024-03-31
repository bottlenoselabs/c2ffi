// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_ignored;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_allowed";
    private const string IgnoredFunctionName = "function_not_allowed";

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/functions/function_ignored/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiFunctionExists(ffi);
            FfiFunctionDoesNotExist(ffi);
        }
    }

    private void FfiFunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        function.CallingConvention.Should().Be("cdecl");
        function.ReturnTypeName.Should().Be("void");
        function.ReturnTypeSizeOf.Should().Be(null);

        function.Parameters.Should().BeEmpty();
    }

    private void FfiFunctionDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction(IgnoredFunctionName);
        function.Should().Be(null);
    }
}
