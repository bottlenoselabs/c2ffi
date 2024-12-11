// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Structs.struct_int;

public class Test : MergeFfisTest
{
    private const string StructName = "struct_int";

    [Fact]
    public void Struct()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/structs/{StructName}/ffi");

        FfiStructExists(ffi);
    }

    private void FfiStructExists(CTestFfiCrossPlatform ffi)
    {
        const string name = $"struct {StructName}";
        var record = ffi.GetRecord(name);
        _ = record.Name.Should().Be(name);
        _ = record.IsStruct.Should().Be(true);
        _ = record.IsUnion.Should().Be(false);

        _ = record.Fields.Length.Should().Be(1);

        var field = record.Fields[0];
        _ = field.Name.Should().Be("a");
        _ = field.OffsetOf.Should().Be(0);
        field.Type.Should().BeInt();
    }
}
