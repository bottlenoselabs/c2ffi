// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1308
#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.MacroObjects.macro_object_string;

public class Test : MergeFfisTest
{
    private const string MacroObjectName = "MACRO_OBJECT_STRING";

    [Fact]
    public void MacroObject()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/macro_objects/{MacroObjectName.ToLowerInvariant()}/ffi");
        FfiMacroObjectExists(ffi);
    }

    private void FfiMacroObjectExists(CTestFfiCrossPlatform ffi)
    {
        var macroObject = ffi.GetMacroObject(MacroObjectName);
        macroObject.Name.Should().Be(MacroObjectName);
        macroObject.TypeName.Should().Be("const char *");
        macroObject.Value.Should().Be("\"42\"");
    }
}
