// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.MacroObjects.macro_object_no_value;

public class Test : ExtractFfiTest
{
    private const string MacroObjectName = "MACRO_OBJECT_NO_VALUE";

    [Fact]
    public void MacroObject()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/macro_objects/macro_object_no_value/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FfiMacroObjectDoesNotExist(ffi);
        }
    }

    private void FfiMacroObjectDoesNotExist(CTestFfiTargetPlatform ffi)
    {
        var macroObject = ffi.TryGetMacroObject(MacroObjectName);
        macroObject.Should().Be(null);
    }
}
