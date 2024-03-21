// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace c2json.Tests.EndToEnd.FunctionInt;

public class FunctionIntTest : AbstractSyntaxTreeTest
{
    [Fact]
    public void FunctionExists()
    {
        var asts = GetAbstractSyntaxTrees("src/c/tests/function_int/config.json");
        Assert.True(asts.Length > 0);

        foreach (var ast in asts)
        {
            var function = ast.GetFunction("function_int");
            Assert.True(function.ReturnTypeName == "int");
            Assert.True(function.Parameters.IsDefaultOrEmpty);
        }
    }
}
