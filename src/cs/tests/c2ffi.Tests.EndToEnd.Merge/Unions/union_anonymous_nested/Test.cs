// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Merge.Unions.union_anonymous_nested;

public class Test : MergeFfisTest
{
    private const string UnionName = "union_anonymous_nested";

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
        fieldType.Name.Should().Be(name + "_ANONYMOUS_0");
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
        anonymousField1.Name.Should().BeEmpty();
        anonymousField1.OffsetOf.Should().Be(0);
        anonymousField1.Type.Name.Should().Be(anonymousUnion.Name + "_ANONYMOUS_0");
        anonymousField1.Type.SizeOf.Should().Be(4);
        anonymousField1.Type.AlignOf.Should().Be(4);
        anonymousField1.Type.IsAnonymous.Should().BeTrue();
        anonymousField1.Type.InnerType.Should().BeNull();

        var anonymousField2 = anonymousUnion.Fields[1];
        anonymousField2.Name.Should().BeEmpty();
        anonymousField2.OffsetOf.Should().Be(0);
        anonymousField2.Type.Name.Should().Be(anonymousUnion.Name + "_ANONYMOUS_1");
        anonymousField2.Type.SizeOf.Should().Be(4);
        anonymousField2.Type.AlignOf.Should().Be(4);
        anonymousField2.Type.IsAnonymous.Should().BeTrue();
        anonymousField2.Type.InnerType.Should().BeNull();

        var nestedAnonymousUnion1 = ffi.GetRecord(anonymousField1.Type.Name);
        nestedAnonymousUnion1.IsStruct.Should().BeFalse();
        nestedAnonymousUnion1.IsUnion.Should().BeTrue();
        nestedAnonymousUnion1.SizeOf.Should().Be(4);
        nestedAnonymousUnion1.AlignOf.Should().Be(4);
        nestedAnonymousUnion1.IsAnonymous.Should().BeTrue();
        nestedAnonymousUnion1.Fields.Length.Should().Be(2);

        var nestedAnonymousUnion1Field1 = nestedAnonymousUnion1.Fields[0];
        nestedAnonymousUnion1Field1.Name.Should().Be("a");
        nestedAnonymousUnion1Field1.Type.Should().BeChar();
        nestedAnonymousUnion1Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousUnion1Field2 = nestedAnonymousUnion1.Fields[1];
        nestedAnonymousUnion1Field2.Name.Should().Be("b");
        nestedAnonymousUnion1Field2.Type.Should().BeInt();
        nestedAnonymousUnion1Field2.OffsetOf.Should().Be(0);

        var nestedAnonymousUnion2 = ffi.GetRecord(anonymousField2.Type.Name);
        nestedAnonymousUnion2.IsStruct.Should().BeFalse();
        nestedAnonymousUnion2.IsUnion.Should().BeTrue();
        nestedAnonymousUnion2.SizeOf.Should().Be(4);
        nestedAnonymousUnion2.AlignOf.Should().Be(4);
        nestedAnonymousUnion2.IsAnonymous.Should().BeTrue();
        nestedAnonymousUnion2.Fields.Length.Should().Be(2);

        var nestedAnonymousUnion2Field1 = nestedAnonymousUnion2.Fields[0];
        nestedAnonymousUnion2Field1.Name.Should().Be("c");
        nestedAnonymousUnion2Field1.Type.Should().BeChar();
        nestedAnonymousUnion2Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousUnion2Field2 = nestedAnonymousUnion2.Fields[1];
        nestedAnonymousUnion2Field2.Name.Should().Be("d");
        nestedAnonymousUnion2Field2.Type.Should().BeInt();
        nestedAnonymousUnion2Field2.OffsetOf.Should().Be(0);
    }
}
