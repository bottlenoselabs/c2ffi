// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace c2ffi.Data.Serialization;

/// <summary>
///     Provides a converter that serializes and deserializes <see cref="TargetPlatform" /> object instances to and from
///     JSON.
/// </summary>
public class TargetPlatformJsonConverter : JsonConverter<TargetPlatform>
{
    /// <summary>
    ///     Reads and converts the JSON to a <see cref="TargetPlatform" /> object instance.
    /// </summary>
    /// <param name="reader">The reader to read with.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted object instance.</returns>
    public override TargetPlatform Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return TargetPlatform.Unknown;
        }

        var result = new TargetPlatform(value);
        return result;
    }

    /// <summary>
    ///     Writes a <see cref="TargetPlatform" /> object instance in JSON form.
    /// </summary>
    /// <param name="writer">The writer to write with.</param>
    /// <param name="value">The object instance to convert.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(
        Utf8JsonWriter writer,
        TargetPlatform value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ClangTargetTriple);
    }
}
