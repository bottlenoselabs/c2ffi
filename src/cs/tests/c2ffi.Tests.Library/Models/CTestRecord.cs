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
public class CTestRecord
{
    public string Name { get; }

    public int SizeOf { get; }

    public int AlignOf { get; }

    public bool IsUnion { get; }

    public bool IsAnonymous { get;  }

    public bool IsStruct => !IsUnion;

    public ImmutableArray<CTestRecordField> Fields { get; }

    public CTestRecord(CRecord record)
    {
        Name = record.Name;
        SizeOf = record.SizeOf;
        AlignOf = record.AlignOf;
        IsUnion = record.RecordKind == CRecordKind.Union;
        IsAnonymous = record.IsAnonymous ?? false;
        Fields = record.Fields.Select(field => new CTestRecordField(field)).ToImmutableArray();
    }

    public override string ToString()
    {
        return Name;
    }
}
