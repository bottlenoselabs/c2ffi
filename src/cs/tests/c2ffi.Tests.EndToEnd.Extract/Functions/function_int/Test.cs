// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_int;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_int";

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

    private void FfiFunctionExists(CTestFfiTargetPlatform ffi)
    {
        var function = ffi.GetFunction(FunctionName);
        Assert.True(function.CallingConvention == "cdecl");
        Assert.True(function.ReturnTypeName == "int");

        Assert.True(function.Parameters.IsDefaultOrEmpty);
    }
}
