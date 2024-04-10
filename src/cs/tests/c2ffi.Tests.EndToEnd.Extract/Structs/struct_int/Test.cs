// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Models;
using FluentAssertions;

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
        record.Name.Should().Be(name);
        record.IsStruct.Should().BeTrue();
        record.IsUnion.Should().BeFalse();
        record.IsAnonymous.Should().BeFalse();

        record.Fields.Length.Should().Be(1);

        var field = record.Fields[0];
        field.Name.Should().Be("a");
        field.OffsetOf.Should().Be(0);
        field.Type.Should().BeInt();
    }
}
