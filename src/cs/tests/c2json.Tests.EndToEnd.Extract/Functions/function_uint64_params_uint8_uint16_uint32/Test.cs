// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2json.Tests.Library.Models;

#pragma warning disable CA1707

namespace c2json.Tests.EndToEnd.Extract.Functions.function_uint64_params_uint8_uint16_uint32;

public class Test : ExtractAbstractSyntaxTreeTest
{
    private const string FunctionName = "function_uint64_params_uint8_uint16_uint32";

    [Fact]
    public void FunctionExists()
    {
        var asts = GetAbstractSyntaxTrees(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(asts.Length > 0);

        foreach (var ast in asts)
        {
            AstFunctionExists(ast);
        }
    }

    private static void AstFunctionExists(CTestAbstractSyntaxTree ast)
    {
        var function = ast.GetFunction(FunctionName);
        Assert.True(function.CallingConvention == "cdecl");
        Assert.True(function.ReturnTypeName == "uint64_t");

        Assert.True(function.Parameters.Length == 3);

        var parameter1 = function.Parameters[0];
        Assert.True(parameter1.Name == "a");
        Assert.True(parameter1.TypeName == "uint8_t");

        var parameter2 = function.Parameters[1];
        Assert.True(parameter2.Name == "b");
        Assert.True(parameter2.TypeName == "uint16_t");

        var parameter3 = function.Parameters[2];
        Assert.True(parameter3.Name == "c");
        Assert.True(parameter3.TypeName == "uint32_t");
    }
}