// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Unions.union_int_int;

public class Test : MergeFfisTest
{
    private const string UnionName = "union_int_int";

    [Fact]
    public void Union()
    {
        var ffi = GetCrossPlatformFfi(
            $"src/c/tests/unions/{UnionName}/ffi");
        UnionExists(ffi);
    }

    private void UnionExists(CTestFfiCrossPlatform ffi)
    {
        const string name = $"union {UnionName}";
        var record = ffi.GetRecord(name);
        _ = record.Name.Should().Be(name);
        _ = record.IsStruct.Should().Be(false);
        _ = record.IsUnion.Should().Be(true);

        _ = record.Fields.Length.Should().Be(2);

        var field1 = record.Fields[0];
        _ = field1.Name.Should().Be("a");
        _ = field1.OffsetOf.Should().Be(0);
        _ = field1.Type.Name.Should().Be("int");
        _ = field1.Type.SizeOf.Should().Be(4);
        _ = field1.Type.AlignOf.Should().Be(4);
        _ = field1.Type.InnerType.Should().BeNull();

        var field2 = record.Fields[1];
        _ = field2.Name.Should().Be("b");
        _ = field2.OffsetOf.Should().Be(0);
        _ = field2.Type.Name.Should().Be("int");
        _ = field2.Type.SizeOf.Should().Be(4);
        _ = field2.Type.AlignOf.Should().Be(4);
        _ = field2.Type.InnerType.Should().BeNull();
    }
}
