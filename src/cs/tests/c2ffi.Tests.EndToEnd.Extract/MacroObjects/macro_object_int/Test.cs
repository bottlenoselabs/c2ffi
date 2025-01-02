// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.MacroObjects.macro_object_int;

public class Test : ExtractFfiTest
{
    private const string MacroObjectName = "MACRO_OBJECT_INT";

    [Fact]
    public void MacroObject()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/macro_objects/{MacroObjectName.ToLowerInvariant()}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            MacroObjectExists(ffi);
        }
    }

    private void MacroObjectExists(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.GetMacroObject(MacroObjectName);
        _ = macroObject.Name.Should().Be(MacroObjectName);
        _ = macroObject.Value.Should().Be("42");
        _ = macroObject.Type.Name.Should().Be("int");
        _ = macroObject.Type.InnerType.Should().BeNull();
    }
}
