// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Unions.union_anonymous_nested;

public class Test : ExtractFfiTest
{
    private const string UnionName = "union_anonymous_nested";

    [Fact]
    public void Union()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/unions/{UnionName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            UnionExists(ffi);
        }
    }

    private void UnionExists(CTestFfiTargetPlatform ffi)
    {
        const string name = $"union {UnionName}";
        var union = ffi.GetRecord(name);
        _ = union.Name.Should().Be(name);
        _ = union.IsStruct.Should().BeFalse();
        _ = union.IsUnion.Should().BeTrue();
        _ = union.IsAnonymous.Should().BeFalse();

        _ = union.Fields.Length.Should().Be(1);

        var field = union.Fields[0];
        _ = field.Name.Should().BeEmpty();
        _ = field.OffsetOf.Should().Be(0);

        var fieldType = field.Type;
        _ = fieldType.Name.Should().Be(name + "_ANONYMOUS_0");
        _ = fieldType.SizeOf.Should().Be(4);
        _ = fieldType.AlignOf.Should().Be(4);
        _ = fieldType.IsAnonymous.Should().BeTrue();
        _ = fieldType.InnerType.Should().BeNull();

        var anonymousUnion = ffi.GetRecord(fieldType.Name);
        _ = anonymousUnion.IsStruct.Should().BeFalse();
        _ = anonymousUnion.IsUnion.Should().BeTrue();
        _ = anonymousUnion.SizeOf.Should().Be(4);
        _ = anonymousUnion.AlignOf.Should().Be(4);
        _ = anonymousUnion.IsAnonymous.Should().BeTrue();
        _ = anonymousUnion.Fields.Length.Should().Be(2);

        var anonymousField1 = anonymousUnion.Fields[0];
        _ = anonymousField1.Name.Should().BeEmpty();
        _ = anonymousField1.OffsetOf.Should().Be(0);
        _ = anonymousField1.Type.Name.Should().Be(anonymousUnion.Name + "_ANONYMOUS_0");
        _ = anonymousField1.Type.SizeOf.Should().Be(4);
        _ = anonymousField1.Type.AlignOf.Should().Be(4);
        _ = anonymousField1.Type.IsAnonymous.Should().BeTrue();
        _ = anonymousField1.Type.InnerType.Should().BeNull();

        var anonymousField2 = anonymousUnion.Fields[1];
        _ = anonymousField2.Name.Should().BeEmpty();
        _ = anonymousField2.OffsetOf.Should().Be(0);
        _ = anonymousField2.Type.Name.Should().Be(anonymousUnion.Name + "_ANONYMOUS_1");
        _ = anonymousField2.Type.SizeOf.Should().Be(4);
        _ = anonymousField2.Type.AlignOf.Should().Be(4);
        _ = anonymousField2.Type.IsAnonymous.Should().BeTrue();
        _ = anonymousField2.Type.InnerType.Should().BeNull();

        var nestedAnonymousUnion1 = ffi.GetRecord(anonymousField1.Type.Name);
        _ = nestedAnonymousUnion1.IsStruct.Should().BeFalse();
        _ = nestedAnonymousUnion1.IsUnion.Should().BeTrue();
        _ = nestedAnonymousUnion1.SizeOf.Should().Be(4);
        _ = nestedAnonymousUnion1.AlignOf.Should().Be(4);
        _ = nestedAnonymousUnion1.IsAnonymous.Should().BeTrue();
        _ = nestedAnonymousUnion1.Fields.Length.Should().Be(2);

        var nestedAnonymousUnion1Field1 = nestedAnonymousUnion1.Fields[0];
        _ = nestedAnonymousUnion1Field1.Name.Should().Be("a");
        nestedAnonymousUnion1Field1.Type.Should().BeChar();
        _ = nestedAnonymousUnion1Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousUnion1Field2 = nestedAnonymousUnion1.Fields[1];
        _ = nestedAnonymousUnion1Field2.Name.Should().Be("b");
        nestedAnonymousUnion1Field2.Type.Should().BeInt();
        _ = nestedAnonymousUnion1Field2.OffsetOf.Should().Be(0);

        var nestedAnonymousUnion2 = ffi.GetRecord(anonymousField2.Type.Name);
        _ = nestedAnonymousUnion2.IsStruct.Should().BeFalse();
        _ = nestedAnonymousUnion2.IsUnion.Should().BeTrue();
        _ = nestedAnonymousUnion2.SizeOf.Should().Be(4);
        _ = nestedAnonymousUnion2.AlignOf.Should().Be(4);
        _ = nestedAnonymousUnion2.IsAnonymous.Should().BeTrue();
        _ = nestedAnonymousUnion2.Fields.Length.Should().Be(2);

        var nestedAnonymousUnion2Field1 = nestedAnonymousUnion2.Fields[0];
        _ = nestedAnonymousUnion2Field1.Name.Should().Be("c");
        nestedAnonymousUnion2Field1.Type.Should().BeChar();
        _ = nestedAnonymousUnion2Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousUnion2Field2 = nestedAnonymousUnion2.Fields[1];
        _ = nestedAnonymousUnion2Field2.Name.Should().Be("d");
        nestedAnonymousUnion2Field2.Type.Should().BeInt();
        _ = nestedAnonymousUnion2Field2.OffsetOf.Should().Be(0);
    }
}
