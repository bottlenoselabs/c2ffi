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
        @struct.Name.Should().Be(name);
        @struct.IsStruct.Should().BeTrue();
        @struct.IsUnion.Should().BeFalse();
        @struct.IsAnonymous.Should().BeFalse();

        @struct.Fields.Length.Should().Be(1);

        var field = @struct.Fields[0];
        field.Name.Should().BeEmpty();
        field.OffsetOf.Should().Be(0);

        var fieldType = field.Type;
        fieldType.Name.Should().Be(name + "_ANONYMOUS_0");
        fieldType.SizeOf.Should().Be(16);
        fieldType.AlignOf.Should().Be(4);
        fieldType.IsAnonymous.Should().BeTrue();
        fieldType.InnerType.Should().BeNull();

        var anonymousStruct = ffi.GetRecord(fieldType.Name);
        anonymousStruct.IsStruct.Should().BeTrue();
        anonymousStruct.IsUnion.Should().BeFalse();
        anonymousStruct.SizeOf.Should().Be(16);
        anonymousStruct.AlignOf.Should().Be(4);
        anonymousStruct.IsAnonymous.Should().BeTrue();
        anonymousStruct.Fields.Length.Should().Be(2);

        var anonymousField1 = anonymousStruct.Fields[0];
        anonymousField1.Name.Should().BeEmpty();
        anonymousField1.OffsetOf.Should().Be(0);
        anonymousField1.Type.Name.Should().Be(anonymousStruct.Name + "_ANONYMOUS_0");
        anonymousField1.Type.SizeOf.Should().Be(8);
        anonymousField1.Type.AlignOf.Should().Be(4);
        anonymousField1.Type.IsAnonymous.Should().BeTrue();
        anonymousField1.Type.InnerType.Should().BeNull();

        var anonymousField2 = anonymousStruct.Fields[1];
        anonymousField2.Name.Should().BeEmpty();
        anonymousField2.OffsetOf.Should().Be(8);
        anonymousField2.Type.Name.Should().Be(anonymousStruct.Name + "_ANONYMOUS_1");
        anonymousField2.Type.SizeOf.Should().Be(8);
        anonymousField2.Type.AlignOf.Should().Be(4);
        anonymousField2.Type.IsAnonymous.Should().BeTrue();
        anonymousField2.Type.InnerType.Should().BeNull();

        var nestedAnonymousStruct1 = ffi.GetRecord(anonymousField1.Type.Name);
        nestedAnonymousStruct1.IsStruct.Should().BeTrue();
        nestedAnonymousStruct1.IsUnion.Should().BeFalse();
        nestedAnonymousStruct1.SizeOf.Should().Be(8);
        nestedAnonymousStruct1.AlignOf.Should().Be(4);
        nestedAnonymousStruct1.IsAnonymous.Should().BeTrue();
        nestedAnonymousStruct1.Fields.Length.Should().Be(2);

        var nestedAnonymousStruct1Field1 = nestedAnonymousStruct1.Fields[0];
        nestedAnonymousStruct1Field1.Name.Should().Be("a");
        nestedAnonymousStruct1Field1.Type.Should().BeChar();
        nestedAnonymousStruct1Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousStruct1Field2 = nestedAnonymousStruct1.Fields[1];
        nestedAnonymousStruct1Field2.Name.Should().Be("b");
        nestedAnonymousStruct1Field2.Type.Should().BeInt();
        nestedAnonymousStruct1Field2.OffsetOf.Should().Be(4);

        var nestedAnonymousStruct2 = ffi.GetRecord(anonymousField2.Type.Name);
        nestedAnonymousStruct2.IsStruct.Should().BeTrue();
        nestedAnonymousStruct2.IsUnion.Should().BeFalse();
        nestedAnonymousStruct2.SizeOf.Should().Be(8);
        nestedAnonymousStruct2.AlignOf.Should().Be(4);
        nestedAnonymousStruct2.IsAnonymous.Should().BeTrue();
        nestedAnonymousStruct2.Fields.Length.Should().Be(2);

        var nestedAnonymousStruct2Field1 = nestedAnonymousStruct2.Fields[0];
        nestedAnonymousStruct2Field1.Name.Should().Be("c");
        nestedAnonymousStruct2Field1.Type.Should().BeChar();
        nestedAnonymousStruct2Field1.OffsetOf.Should().Be(0);

        var nestedAnonymousStruct2Field2 = nestedAnonymousStruct2.Fields[1];
        nestedAnonymousStruct2Field2.Name.Should().Be("d");
        nestedAnonymousStruct2Field2.Type.Should().BeInt();
        nestedAnonymousStruct2Field2.OffsetOf.Should().Be(4);
    }
}
