// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents an enumeration in a C foreign function interface.
/// </summary>
[PublicAPI]
public class CEnum : CNodeWithLocation
{
    /// <summary>
    ///     Gets or sets the byte size of the enum.
    /// </summary>
    [JsonPropertyName("size_of")]
    public int SizeOf { get; set; }

    /// <summary>
    ///     Gets or sets the enum's values.
    /// </summary>
    [JsonPropertyName("values")]
    public ImmutableArray<CEnumValue> Values { get; set; } = ImmutableArray<CEnumValue>.Empty;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Enum '{Name}': @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CEnum other2)
        {
            return false;
        }

        return SizeOf.Equals(other2.SizeOf) && Values.SequenceEqual(other2.Values);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(SizeOf);

        foreach (var value in Values)
        {
            hashCode.Add(value);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
