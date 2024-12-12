// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Unions.union_anonymous_char_int;

public class Test : ExtractFfiTest
{
    private const string StructName = "union_anonymous_char_int";

    [Fact]
    public void Union()
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
        _ = union.Name.Should().Be(name);
        _ = union.IsStruct.Should().BeFalse();
        _ = union.IsUnion.Should().BeTrue();
        _ = union.IsAnonymous.Should().BeFalse();

        _ = union.Fields.Length.Should().Be(1);

        var field = union.Fields[0];
        _ = field.Name.Should().BeEmpty();
        _ = field.OffsetOf.Should().Be(0);

        var fieldType = field.Type;
        _ = fieldType.Name.Should().Be($"{name}_ANONYMOUS_0");
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
        _ = anonymousField1.Name.Should().Be("a");
        anonymousField1.Type.Should().BeChar();
        _ = anonymousField1.OffsetOf.Should().Be(0);

        var anonymousField2 = anonymousUnion.Fields[1];
        _ = anonymousField2.Name.Should().Be("b");
        anonymousField2.Type.Should().BeInt();
        _ = anonymousField2.OffsetOf.Should().Be(0);
    }
}
