// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Functions.function_int_params_int;

public class Test : ExtractFfiTest
{
    private const string FunctionName = "function_int_params_int";

    [Fact]
    public void FunctionExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ast in ffis)
        {
            AstFunctionExists(ast);
        }
    }

    private static void AstFunctionExists(CTestFfi ast)
    {
        var function = ast.GetFunction(FunctionName);
        Assert.True(function.CallingConvention == "cdecl");
        Assert.True(function.ReturnTypeName == "int");

        Assert.True(function.Parameters.Length == 1);
        var parameter = function.Parameters[0];
        Assert.True(parameter.Name == "a");
        Assert.True(parameter.TypeName == "int");
    }
}
