// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

#pragma warning disable CA1707

namespace c2ffi.Tests.EndToEnd.Extract.Structs.struct_int;

public class Test : ExtractFfiTest
{
    private const string StructName = "struct_int";

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
        var record = ffi.GetRecord(name);
        _ = record.Name.Should().Be(name);
        _ = record.IsStruct.Should().BeTrue();
        _ = record.IsUnion.Should().BeFalse();
        _ = record.IsAnonymous.Should().BeFalse();

        _ = record.Fields.Length.Should().Be(1);

        var field = record.Fields[0];
        _ = field.Name.Should().Be("a");
        _ = field.OffsetOf.Should().Be(0);
        field.Type.Should().BeInt();
    }
}
