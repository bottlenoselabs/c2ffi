// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.MacroObjects.macro_object_invalid;

public class Test : ExtractFfiTest
{
    private const string MacroObjectName = "MACRO_OBJECT_INVALID";

    [Fact]
    public void MacroObject()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/macro_objects/macro_object_invalid/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            MacroObjectDoesNotExist(ffi);
        }
    }

    private void MacroObjectDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.TryGetMacroObject(MacroObjectName);
        _ = macroObject.Should().Be(null);
    }
}
