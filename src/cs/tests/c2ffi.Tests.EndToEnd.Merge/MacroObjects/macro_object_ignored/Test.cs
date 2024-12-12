// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.MacroObjects.macro_object_ignored;

public class Test : MergeFfisTest
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
        var ffi = GetCrossPlatformFfi("src/c/tests/macro_objects/macro_object_ignored/ffi");

        MacroObjectsExist(ffi, _macroObjectNamesThatShouldExist);
        MacroObjectsDoNotExist(ffi, _macroObjectNamesThatShouldNotExist);
    }

    private void MacroObjectsExist(CTestFfiCrossPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var macroObject = ffi.TryGetMacroObject(name);
            _ = macroObject.Should().NotBeNull();
        }
    }

    private void MacroObjectsDoNotExist(CTestFfiCrossPlatform ffi, params string[] names)
    {
        foreach (var name in names)
        {
            var macroObject = ffi.TryGetMacroObject(name);
            _ = macroObject.Should().BeNull();
        }
    }
}
