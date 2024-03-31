// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Variables.variable_allowed;

public class Test : ExtractFfiTest
{
    private const string VariableNameAllowed = "variable_allowed";
    private const string VariableNameNotAllowed = "variable_not_allowed";

    [Fact]
    public void VariableExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/variables/{VariableNameAllowed}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiVariableExists(ffi);
            FfiVariableDoesNotExist(ffi);
        }
    }

    private void FfiVariableExists(CTestFfiTargetPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableNameAllowed);
        variable.Name.Should().Be(VariableNameAllowed);
        variable.TypeName.Should().Be("int");
    }

    private void FfiVariableDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var variable = ffi.TryGetVariable(VariableNameNotAllowed);
        variable.Should().Be(null);
    }
}
