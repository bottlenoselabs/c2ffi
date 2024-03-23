// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.function_int;

public class Test : MergeFfisTest
{
    private const string FunctionName = "function_int";

    [Fact]
    public void FunctionExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/functions/{FunctionName}/ffi");

        var function = ffi.GetFunction(FunctionName);
        Assert.True(function.CallingConvention == "cdecl");
        Assert.True(function.ReturnTypeName == "int");

        Assert.True(function.Parameters.IsDefaultOrEmpty);
    }
}
