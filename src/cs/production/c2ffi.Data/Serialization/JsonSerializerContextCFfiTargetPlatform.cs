// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;

namespace c2ffi.Data.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(CFfiTargetPlatform))]
public partial class JsonSerializerContextCFfiTargetPlatform : JsonSerializerContext
{
}
