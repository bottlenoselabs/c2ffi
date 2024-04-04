// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.MacroObjects.macro_object_ignored;

public class Test : ExtractFfiTest
{
    private const string MacroObjectName = "MACRO_OBJECT_ALLOWED";
    private const string IgnoredMacroObjectName = "MACRO_OBJECT_NOT_ALLOWED";

    [Fact]
    public void MacroObject()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/macro_objects/macro_object_ignored/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiMacroObjectExists(ffi);
            FfiMacroObjectDoesNotExist(ffi);
        }
    }

    private void FfiMacroObjectExists(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.GetMacroObject(MacroObjectName);
        macroObject.Name.Should().Be(MacroObjectName);
        macroObject.Value.Should().Be("42");
        macroObject.Type.Name.Should().Be("int");
        macroObject.Type.InnerType.Should().BeNull();
    }

    private void FfiMacroObjectDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.TryGetMacroObject(IgnoredMacroObjectName);
        macroObject.Should().Be(null);
    }
}
