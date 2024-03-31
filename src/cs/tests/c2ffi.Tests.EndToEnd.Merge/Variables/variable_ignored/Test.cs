// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Variables.variable_allowed;

public class Test : MergeFfisTest
{
    private const string VariableName = "variable_allowed";
    private const string IgnoredVariableName = "variable_not_allowed";

    [Fact]
    public void Variable()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/variables/{VariableName}/ffi");
        FfiVariableExists(ffi);
        FfiVariableDoesNotExist(ffi);
    }

    private void FfiVariableExists(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.GetVariable(VariableName);
        variable.Name.Should().Be(VariableName);
        variable.TypeName.Should().Be("int");
    }

    private void FfiVariableDoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var variable = ffi.TryGetVariable(IgnoredVariableName);
        variable.Should().Be(null);
    }
}
