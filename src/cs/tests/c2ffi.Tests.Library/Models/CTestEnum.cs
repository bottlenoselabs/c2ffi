// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestEnum
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type_integer")]
    public string IntegerType { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public ImmutableArray<CTestEnumValue> Values { get; set; } = ImmutableArray<CTestEnumValue>.Empty;

    public override string ToString()
    {
        return Name;
    }
}
