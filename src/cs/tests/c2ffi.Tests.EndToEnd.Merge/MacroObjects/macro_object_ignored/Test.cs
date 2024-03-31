// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.MacroObjects.macro_object_ignored;

public class Test : MergeFfisTest
{
    private const string MacroObjectName = "MACRO_OBJECT_ALLOWED";
    private const string IgnoredMacroObjectName = "MACRO_OBJECT_NOT_ALLOWED";

    [Fact]
    public void MacroObject()
    {
        var ffi = GetCrossPlatformFfi(
            "src/c/tests/macro_objects/macro_object_ignored/ffi");
        FfiMacroObjectExists(ffi);
        FfiMacroObjectDoesNotExist(ffi);
    }

    private void FfiMacroObjectExists(CTestFfiCrossPlatform ffi)
    {
        var macroObject = ffi.GetMacroObject(MacroObjectName);
        macroObject.Name.Should().Be(MacroObjectName);
        macroObject.TypeName.Should().Be("int");
        macroObject.Value.Should().Be("42");
    }

    private void FfiMacroObjectDoesNotExist(CTestFfiCrossPlatform ffi)
    {
        var macroObject = ffi.TryGetMacroObject(IgnoredMacroObjectName);
        macroObject.Should().Be(null);
    }
}
