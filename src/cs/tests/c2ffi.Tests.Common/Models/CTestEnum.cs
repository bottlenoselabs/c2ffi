// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestEnum(CEnum @enum)
{
    public string Name { get; } = @enum.Name;

    public int SizeOf { get; } = @enum.SizeOf;

    public ImmutableArray<CTestEnumValue> Values { get; } = [..@enum.Values.Select(x => new CTestEnumValue(x))];

    public override string ToString()
    {
        return Name;
    }
}
