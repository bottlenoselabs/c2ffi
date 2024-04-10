// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Structs.struct_anonymous_char_int;

public class Test : ExtractFfiTest
{
    private const string StructName = "struct_anonymous_char_int";

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
        fieldType.Name.Should().NotBeNull();
        fieldType.SizeOf.Should().Be(8);
        fieldType.AlignOf.Should().Be(4);
        fieldType.IsAnonymous.Should().BeTrue();
        fieldType.InnerType.Should().BeNull();

        var anonymousStruct = ffi.GetRecord(fieldType.Name);
        anonymousStruct.IsStruct.Should().BeTrue();
        anonymousStruct.IsUnion.Should().BeFalse();
        anonymousStruct.SizeOf.Should().Be(8);
        anonymousStruct.AlignOf.Should().Be(4);
        anonymousStruct.IsAnonymous.Should().BeTrue();

        anonymousStruct.Fields.Length.Should().Be(2);

        var anonymousField1 = anonymousStruct.Fields[0];
        anonymousField1.Name.Should().Be("a");
        anonymousField1.Type.Should().BeChar();
        anonymousField1.OffsetOf.Should().Be(0);

        var anonymousField2 = anonymousStruct.Fields[1];
        anonymousField2.Name.Should().Be("b");
        anonymousField2.Type.Should().BeInt();
        anonymousField2.OffsetOf.Should().Be(4);
    }
}
