// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2json.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a struct or union field in a C abstract syntax tree.
/// </summary>
[PublicAPI]
public class CRecordField : CNodeWithLocation
{
    /// <summary>
    ///     Gets or sets the name of the field.
    /// </summary>
    [JsonPropertyName("name")]
    public new string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    /// <summary>
    ///     Gets or sets the type information of this field.
    /// </summary>
    [JsonPropertyName("type")]
    public CTypeInfo TypeInfo { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the byte offset of this field.
    /// </summary>
    [JsonPropertyName("offset_of")]
    public int OffsetOf { get; set; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Record field '{Name}': {TypeInfo} @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CRecordField other2)
        {
            return false;
        }

        return TypeInfo.Equals(other2.TypeInfo) && OffsetOf == other2.OffsetOf;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(TypeInfo);
        hashCode.Add(OffsetOf);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
