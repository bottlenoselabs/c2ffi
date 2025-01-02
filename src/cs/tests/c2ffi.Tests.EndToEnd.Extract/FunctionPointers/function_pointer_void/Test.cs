// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.FunctionPointers.function_pointer_void;

public class Test : ExtractFfiTest
{
    private const string FunctionPointerName = "function_pointer_void";

    [Fact]
    public void FunctionPointer()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/function_pointers/{FunctionPointerName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            FunctionPointerExists(ffi);
        }
    }

    private void FunctionPointerExists(CTestFfiTargetPlatform ffi)
    {
        var alias = ffi.GetTypeAlias(FunctionPointerName);
        _ = alias.Name.Should().Be(FunctionPointerName);
        _ = alias.UnderlyingType.NodeKind.Should().Be("functionpointer");
        _ = alias.UnderlyingType.Name.Should().Be("void ()");
        _ = alias.UnderlyingType.SizeOf.Should().Be(8);
        _ = alias.UnderlyingType.AlignOf.Should().Be(8);
        _ = alias.UnderlyingType.InnerType.Should().BeNull();

        var functionPointer = ffi.GetFunctionPointer("void ()");
        _ = functionPointer.Name.Should().Be("void ()");
        _ = functionPointer.CallingConvention.Should().Be("cdecl");
        _ = functionPointer.Parameters.Should().BeEmpty();
        _ = functionPointer.ReturnType.Name.Should().Be("void");
        _ = functionPointer.ReturnType.SizeOf.Should().BeNull();
        _ = functionPointer.ReturnType.AlignOf.Should().BeNull();
        _ = functionPointer.ReturnType.InnerType.Should().BeNull();
        _ = functionPointer.ReturnType.NodeKind.Should().Be("primitive");
    }
}
