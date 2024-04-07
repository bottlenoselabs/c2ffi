// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_internal;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_internal";

    [Fact]
    public void Function()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiFunctionDoesNotExist(ffi);
        }
    }

    private void FfiFunctionDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.TryGetFunction(FunctionName);
        function.Should().BeNull();
    }
}
