// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Unions.union_anonymous_char_int;

public class Test : ExtractFfiTest
{
    private const string StructName = "union_anonymous_char_int";

    [Fact]
    public void Struct()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/unions/{StructName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            UnionExists(ffi);
        }
    }

    private void UnionExists(CTestFfiTargetPlatform ffi)
    {
        const string name = $"union {StructName}";
        var union = ffi.GetRecord(name);
        union.Name.Should().Be(name);
        union.IsStruct.Should().BeFalse();
        union.IsUnion.Should().BeTrue();
        union.IsAnonymous.Should().BeFalse();

        union.Fields.Length.Should().Be(1);

        var field = union.Fields[0];
        field.Name.Should().BeEmpty();
        field.OffsetOf.Should().Be(0);

        var fieldType = field.Type;
        fieldType.Name.Should().NotBeNull();
        fieldType.SizeOf.Should().Be(4);
        fieldType.AlignOf.Should().Be(4);
        fieldType.IsAnonymous.Should().BeTrue();
        fieldType.InnerType.Should().BeNull();

        var anonymousUnion = ffi.GetRecord(fieldType.Name);
        anonymousUnion.IsStruct.Should().BeFalse();
        anonymousUnion.IsUnion.Should().BeTrue();
        anonymousUnion.SizeOf.Should().Be(4);
        anonymousUnion.AlignOf.Should().Be(4);
        anonymousUnion.IsAnonymous.Should().BeTrue();

        anonymousUnion.Fields.Length.Should().Be(2);

        var anonymousField1 = anonymousUnion.Fields[0];
        anonymousField1.Name.Should().Be("a");
        anonymousField1.Type.Should().BeChar();
        anonymousField1.OffsetOf.Should().Be(0);

        var anonymousField2 = anonymousUnion.Fields[1];
        anonymousField2.Name.Should().Be("b");
        anonymousField2.Type.Should().BeInt();
        anonymousField2.OffsetOf.Should().Be(0);
    }
}
