// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Structs.struct_anonymous_nested;

public class Test : ExtractFfiTest
{
    private const string StructName = "struct_anonymous_nested";

    [Fact]
    public void Struct()
    {
        var ffis = GetTargetPlatformFfis(
            $"src/c/tests/structs/{StructName}/config.json");
        Assert.True(ffis.Length > 0);

        foreach (var ffi in ffis)
        {
            StructExists(ffi);
        }
    }

    private void StructExists(CTestFfiTargetPlatform ffi)
    {
        const string name = $"struct {StructName}";
        var @struct = ffi.GetRecord(name);
        _ = @struct.Name.Should().Be(name);
        _ = @struct.IsStruct.Should().BeTrue();
        _ = @struct.IsUnion.Should().BeFalse();
        _ = @struct.IsAnonymous.Should().BeFalse();

        _ = @struct.Fields.Length.Should().Be(1);

        var field = @struct.Fields[0];
        _ = field.Name.Should().BeEmpty();
        _ = field.OffsetOf.Should().Be(0);

        var fieldType = field.Type;
        _ = fieldType.Name.Should().Be(string.Empty);
        _ = fieldType.SizeOf.Should().Be(16);
        _ = fieldType.AlignOf.Should().Be(4);
        _ = fieldType.IsAnonymous.Should().BeTrue();
        _ = fieldType.InnerType.Should().BeNull();

        _ = @struct.NestedRecords.Should().HaveCount(1);
        var anonymousStruct = @struct.NestedRecords[0];
        _ = anonymousStruct.IsStruct.Should().BeTrue();
        _ = anonymousStruct.IsUnion.Should().BeFalse();
        _ = anonymousStruct.SizeOf.Should().Be(16);
        _ = anonymousStruct.AlignOf.Should().Be(4);
        _ = anonymousStruct.IsAnonymous.Should().BeTrue();
        _ = anonymousStruct.Fields.Length.Should().Be(2);

        var anonymousField1 = anonymousStruct.Fields[0];
        _ = anonymousField1.Name.Should().BeEmpty();
        _ = anonymousField1.OffsetOf.Should().Be(0);
        _ = anonymousField1.Type.Name.Should().Be(string.Empty);
        _ = anonymousField1.Type.SizeOf.Should().Be(8);
        _ = anonymousField1.Type.AlignOf.Should().Be(4);
        _ = anonymousField1.Type.IsAnonymous.Should().BeTrue();
        _ = anonymousField1.Type.InnerType.Should().BeNull();

        var anonymousField2 = anonymousStruct.Fields[1];
        _ = anonymousField2.Name.Should().BeEmpty();
        _ = anonymousField2.OffsetOf.Should().Be(8);
        _ = anonymousField2.Type.Name.Should().Be(string.Empty);
        _ = anonymousField2.Type.SizeOf.Should().Be(8);
        _ = anonymousField2.Type.AlignOf.Should().Be(4);
        _ = anonymousField2.Type.IsAnonymous.Should().BeTrue();
        _ = anonymousField2.Type.InnerType.Should().BeNull();

        _ = anonymousStruct.NestedRecords.Should().HaveCount(2);
        var nestedAnonymousStruct1 = anonymousStruct.NestedRecords[0];
        _ = nestedAnonymousStruct1.IsStruct.Should().BeTrue();
        _ = nestedAnonymousStruct1.IsUnion.Should().BeFalse();
        _ = nestedAnonymousStruct1.SizeOf.Should().Be(8);
        _ = nestedAnonymousStruct1.AlignOf.Should().Be(4);
        _ = nestedAnonymousStruct1.IsAnonymous.Should().BeTrue();
        _ = nestedAnonymousStruct1.Fields.Length.Should().Be(2);

        var nestedAnonymousStruct1Field1 = nestedAnonymousStruct1.Fields[0];
        _ = nestedAnonymousStruct1Field1.Name.Should().Be("a");
        nestedAnonymousStruct1Field1.Type.Should().BeChar();
        _ = nestedAnonymousStruct1Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousStruct1Field2 = nestedAnonymousStruct1.Fields[1];
        _ = nestedAnonymousStruct1Field2.Name.Should().Be("b");
        nestedAnonymousStruct1Field2.Type.Should().BeInt();
        _ = nestedAnonymousStruct1Field2.OffsetOf.Should().Be(4);

        var nestedAnonymousStruct2 = anonymousStruct.NestedRecords[1];
        _ = nestedAnonymousStruct2.IsStruct.Should().BeTrue();
        _ = nestedAnonymousStruct2.IsUnion.Should().BeFalse();
        _ = nestedAnonymousStruct2.SizeOf.Should().Be(8);
        _ = nestedAnonymousStruct2.AlignOf.Should().Be(4);
        _ = nestedAnonymousStruct2.IsAnonymous.Should().BeTrue();
        _ = nestedAnonymousStruct2.Fields.Length.Should().Be(2);

        var nestedAnonymousStruct2Field1 = nestedAnonymousStruct2.Fields[0];
        _ = nestedAnonymousStruct2Field1.Name.Should().Be("c");
        nestedAnonymousStruct2Field1.Type.Should().BeChar();
        _ = nestedAnonymousStruct2Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousStruct2Field2 = nestedAnonymousStruct2.Fields[1];
        _ = nestedAnonymousStruct2Field2.Name.Should().Be("d");
        nestedAnonymousStruct2Field2.Type.Should().BeInt();
        _ = nestedAnonymousStruct2Field2.OffsetOf.Should().Be(4);
    }
}
