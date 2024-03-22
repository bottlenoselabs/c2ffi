// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2json.Tests.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestTypeAlias
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("underlyingName")]
    public string UnderlyingName { get; set; } = string.Empty;

    [JsonPropertyName("underlyingKind")]
    public string UnderlyingKind { get; set; } = string.Empty;
}
