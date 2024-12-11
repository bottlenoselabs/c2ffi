// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Variables.variable_int;

public class Test : MergeFfisTest
{
    private const string VariableName = "variable_int";

    [Fact]
    public void Variable()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/variables/{VariableName}/ffi");
        FfiVariableExists(ffi);
    }

    private void FfiVariableExists(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableName);
        _ = variable.Name.Should().Be(VariableName);
        _ = variable.TypeName.Should().Be("int");
    }
}
