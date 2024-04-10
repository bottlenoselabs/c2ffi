// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestRecordField
{
    public string Name { get; }

    public int OffsetOf { get; }

    public CTestType Type { get; }

    public CTestRecordField(CRecordField field)
    {
        Name = field.Name;
        OffsetOf = field.OffsetOf;
        Type = new CTestType(field.Type);
    }

    public override string ToString()
    {
        return Name;
    }
}
