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
        _ = fieldType.SizeOf.Should().Be(8);
        _ = fieldType.AlignOf.Should().Be(4);
        _ = fieldType.IsAnonymous.Should().BeTrue();
        _ = fieldType.InnerType.Should().BeNull();

        _ = @struct.NestedRecords.Should().HaveCount(1);
        var anonymousStruct = @struct.NestedRecords[0];
        _ = anonymousStruct.IsStruct.Should().BeTrue();
        _ = anonymousStruct.IsUnion.Should().BeFalse();
        _ = anonymousStruct.SizeOf.Should().Be(8);
        _ = anonymousStruct.AlignOf.Should().Be(4);
        _ = anonymousStruct.IsAnonymous.Should().BeTrue();

        _ = anonymousStruct.Fields.Length.Should().Be(2);

        var anonymousField1 = anonymousStruct.Fields[0];
        _ = anonymousField1.Name.Should().Be("a");
        anonymousField1.Type.Should().BeChar();
        _ = anonymousField1.OffsetOf.Should().Be(0);

        var anonymousField2 = anonymousStruct.Fields[1];
        _ = anonymousField2.Name.Should().Be("b");
        anonymousField2.Type.Should().BeInt();
        _ = anonymousField2.OffsetOf.Should().Be(4);
    }
}
