// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Variables.variable_int;

public class Test : ExtractFfiTest
{
    private const string VariableName = "variable_int";

    [Fact]
    public void Variable()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/variables/{VariableName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            VariableExists(ffi);
        }
    }

    private void VariableExists(CTestFfiTargetPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableName);
        _ = variable.Name.Should().Be(VariableName);
        _ = variable.TypeName.Should().Be("int");
    }
}
