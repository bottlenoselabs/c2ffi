// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;
using Xunit;

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.FunctionPointers.function_pointer_void;

public class Test : MergeFfisTest
{
    private const string FunctionPointerName = "function_pointer_void";

    [Fact]
    public void FunctionPointer()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/function_pointers/{FunctionPointerName}/ffi");

        FfiFunctionPointer(ffi);
    }

    private void FfiFunctionPointer(CTestFfiCrossPlatform ffi)
    {
        var alias = ffi.GetTypeAlias(FunctionPointerName);
        alias.Name.Should().Be(FunctionPointerName);
        alias.UnderlyingType.NodeKind.Should().Be("functionpointer");
        alias.UnderlyingType.Name.Should().Be("void ()");
        alias.UnderlyingType.SizeOf.Should().Be(8);
        alias.UnderlyingType.AlignOf.Should().Be(8);
        alias.UnderlyingType.InnerType.Should().BeNull();

        var functionPointer = ffi.GetFunctionPointer("void ()");
        functionPointer.Name.Should().Be("void ()");
        functionPointer.CallingConvention.Should().Be("cdecl");
        functionPointer.Parameters.Should().BeEmpty();
        functionPointer.ReturnType.Name.Should().Be("void");
        functionPointer.ReturnType.SizeOf.Should().BeNull();
        functionPointer.ReturnType.AlignOf.Should().BeNull();
        functionPointer.ReturnType.InnerType.Should().BeNull();
        functionPointer.ReturnType.NodeKind.Should().Be("primitive");
    }
}
