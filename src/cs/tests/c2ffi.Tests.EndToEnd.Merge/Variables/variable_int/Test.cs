// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Variables.variable_int;

public class Test : MergeFfisTest
{
    private const string VariableName = "variable_int";

    [Fact]
    public void VariableExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/variables/{VariableName}/ffi");
        FfiVariableExists(ffi);
    }

    private void FfiVariableExists(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableName);
        Assert.True(variable.Name == VariableName);
        Assert.True(variable.TypeName == "int");
    }
}
