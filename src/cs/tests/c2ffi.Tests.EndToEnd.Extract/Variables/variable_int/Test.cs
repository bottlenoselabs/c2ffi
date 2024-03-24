// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Variables.variable_int;

public class Test : ExtractFfiTest
{
    private const string VariableName = "variable_int";

    [Fact]
    public void VariableExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/variables/{VariableName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiVariableExists(ffi);
        }
    }

    private void FfiVariableExists(CTestFfiTargetPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableName);
        Assert.True(variable.Name == VariableName);
        Assert.True(variable.TypeName == "int");
    }
}
