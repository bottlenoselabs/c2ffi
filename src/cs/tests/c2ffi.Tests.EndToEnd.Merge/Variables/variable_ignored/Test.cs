// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Variables.variable_ignored;

public class Test : MergeFfisTest
{
    private readonly string[] _variableNamesThatShouldExist =
    [
        "variable_allowed"
    ];

    private readonly string[] _variableNamesThatShouldNotExist =
    [
        "variable_not_allowed",
        "variable_ignored_1",
        "variable_ignored_2"
    ];

    [Fact]
    public void Variable()
    {
        var ffi = GetCrossPlatformFfi("src/c/tests/variables/variable_ignored/ffi");

        VariablesExist(ffi, _variableNamesThatShouldExist);
        VariablesDoNotExist(ffi, _variableNamesThatShouldNotExist);
    }

    private void VariablesExist(CTestFfiCrossPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var variable = ffi.TryGetVariable(name);
            variable.Should().NotBeNull();
        }
    }

    private void VariablesDoNotExist(CTestFfiCrossPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var variable = ffi.TryGetVariable(name);
            variable.Should().BeNull();
        }
    }
}
