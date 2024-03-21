// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2json.Tests.EndToEnd.Functions.function_int_params_int;

public class Test : AbstractSyntaxTreeTest
{
    private const string FunctionName = "function_int_params_int";

    [Fact]
    public void FunctionExists()
    {
        var asts = GetAbstractSyntaxTrees(
            $"src/c/tests/functions/{FunctionName}/config.json");
        Assert.True(asts.Length > 0);

        foreach (var ast in asts)
        {
            var function = ast.GetFunction(FunctionName);
            Assert.True(function.ReturnTypeName == "int");

            Assert.True(function.Parameters.Length == 1);
            var parameter = function.Parameters[0];
            Assert.True(parameter.Name == "a");
            Assert.True(parameter.TypeName == "int");
        }
    }
}
