// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestRecord(CRecord record)
{
    public string Name { get; } = record.Name;

    public int SizeOf { get; } = record.SizeOf;

    public int AlignOf { get; } = record.AlignOf;

    public bool IsUnion { get; } = record.RecordKind == CRecordKind.Union;

    public bool IsAnonymous { get; } = record.IsAnonymous;

    public bool IsStruct => !IsUnion;

    public ImmutableArray<CTestRecordField> Fields { get; } = [..record.Fields.Select(field => new CTestRecordField(field))];

    public override string ToString()
    {
        return Name;
    }
}
