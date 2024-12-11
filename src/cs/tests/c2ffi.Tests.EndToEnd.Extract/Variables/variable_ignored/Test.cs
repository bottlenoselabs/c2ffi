// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Variables.variable_ignored;

public class Test : ExtractFfiTest
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
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/variables/variable_ignored/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            VariablesExist(ffi, _variableNamesThatShouldExist);
            VariablesDoNotExist(ffi, _variableNamesThatShouldNotExist);
        }
    }

    private void VariablesExist(CTestFfiTargetPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var variable = ffi.TryGetVariable(name);
            _ = variable.Should().NotBeNull();
        }
    }

    private void VariablesDoNotExist(CTestFfiTargetPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var variable = ffi.TryGetVariable(name);
            _ = variable.Should().BeNull();
        }
    }
}
