// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Variables.variable_allowed;

public class Test : MergeFfisTest
{
    private const string VariableNameAllowed = "variable_allowed";
    private const string VariableNameNotAllowed = "variable_not_allowed";

    [Fact]
    public void VariableExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/variables/{VariableNameAllowed}/ffi");
        FfiVariableExists(ffi);
        FfiVariableDoesNotExist(ffi);
    }

    private void FfiVariableExists(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableNameAllowed);
        variable.Name.Should().Be(VariableNameAllowed);
        variable.TypeName.Should().Be("int");
    }

    private void FfiVariableDoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.TryGetVariable(VariableNameNotAllowed);
        variable.Should().Be(null);
    }
}
