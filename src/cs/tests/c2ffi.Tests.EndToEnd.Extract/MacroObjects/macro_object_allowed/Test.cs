// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.MacroObjects.macro_object_allowed;

public class Test : ExtractFfiTest
{
    private const string AllowedMacroObjectName = "MACRO_OBJECT_ALLOWED";
    private const string NotAllowedMacroObjectName = "MACRO_OBJECT_NOT_ALLOWED";

    [Fact]
    public void MacroObjectExists()
    {
        var ffis = GetFfis(
            $"src/c/tests/macro_objects/macro_object_allowed/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiMacroObjectExists(ffi);
            FfiMacroObjectDoesNotExist(ffi);
        }
    }

    private void FfiMacroObjectExists(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.GetMacroObject(AllowedMacroObjectName);
        macroObject.Name.Should().Be(AllowedMacroObjectName);
        macroObject.TypeName.Should().Be("int");
        macroObject.Value.Should().Be("42");
    }

    private void FfiMacroObjectDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.TryGetMacroObject(NotAllowedMacroObjectName);
        macroObject.Should().Be(null);
    }
}
