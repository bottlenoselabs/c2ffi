// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.MacroObjects.macro_object_int;

public class Test : MergeFfisTest
{
    private const string MacroObjectName = "MACRO_OBJECT_INT";

    [Fact]
    public void MacroObjectExists()
    {
        var ffi = GetFfi(
            $"src/c/tests/macro_objects/{MacroObjectName}/ffi");
        FfiMacroObjectExists(ffi);
    }

    private void FfiMacroObjectExists(CTestFfiCrossPlatform ffi)
    {
        var macroObject = ffi.GetMacroObject(MacroObjectName);
        Assert.True(macroObject.Name == MacroObjectName);
        Assert.True(macroObject.TypeName == "int");
        Assert.True(macroObject.Value == "42");
    }
}
