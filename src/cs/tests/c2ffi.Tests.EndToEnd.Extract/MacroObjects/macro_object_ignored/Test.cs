// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.MacroObjects.macro_object_ignored;

public class Test : ExtractFfiTest
{
    private readonly string[] _macroObjectNamesThatShouldExist =
    [
        "MACRO_OBJECT_ALLOWED"
    ];

    private readonly string[] _macroObjectNamesThatShouldNotExist =
    [
        "MACRO_OBJECT_NOT_ALLOWED",
        "MACRO_OBJECT_IGNORED_1",
        "MACRO_OBJECT_IGNORED_2"
    ];

    [Fact]
    public void MacroObject()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/macro_objects/macro_object_ignored/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            MacroObjectsExist(ffi, _macroObjectNamesThatShouldExist);
            MacroObjectsDoNotExist(ffi, _macroObjectNamesThatShouldNotExist);
        }
    }

    private void MacroObjectsExist(CTestFfiTargetPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var macroObject = ffi.TryGetMacroObject(name);
            macroObject.Should().NotBeNull();
        }
    }

    private void MacroObjectsDoNotExist(CTestFfiTargetPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var macroObject = ffi.TryGetMacroObject(name);
            macroObject.Should().BeNull();
        }
    }
}
