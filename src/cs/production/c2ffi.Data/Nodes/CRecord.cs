// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a struct or union field in a C foreign function interface.
/// </summary>
[PublicAPI]
public class CRecord : CNodeWithLocation
{
    /// <summary>
    ///     Gets or sets the kind of record.
    /// </summary>
    [JsonPropertyName("record_kind")]
    public CRecordKind RecordKind { get; set; }

    /// <summary>
    ///     Gets or sets the byte size.
    /// </summary>
    [JsonPropertyName("size_of")]
    public int SizeOf { get; set; }

    /// <summary>
    ///     Gets or sets the alignment byte size.
    /// </summary>
    [JsonPropertyName("align_of")]
    public int AlignOf { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the C type is anonymous.
    /// </summary>
    [JsonPropertyName("is_anonymous")]
    public bool IsAnonymous { get; set; }

    /// <summary>
    ///     Gets or sets the fields.
    /// </summary>
    [JsonPropertyName("fields")]
    public ImmutableArray<CRecordField> Fields { get; set; } = ImmutableArray<CRecordField>.Empty;

    /// <summary>
    ///     Gets or sets the nested records.
    /// </summary>
    [JsonPropertyName("nested")]
    public ImmutableArray<CRecord> NestedRecords { get; set; } = ImmutableArray<CRecord>.Empty;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{RecordKind} {Name} @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CRecord other2)
        {
            return false;
        }

        var kindsAreEqual = RecordKind == other2.RecordKind;
        var sizesAreEqual = SizeOf == other2.SizeOf;
        var alignmentsAreEqual = AlignOf == other2.AlignOf;
        var fieldsAreEqual = Fields.SequenceEqual(other2.Fields);

        return kindsAreEqual &&
               sizesAreEqual &&
               alignmentsAreEqual &&
               fieldsAreEqual;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(RecordKind);
        hashCode.Add(SizeOf);
        hashCode.Add(AlignOf);

        foreach (var field in Fields)
        {
            hashCode.Add(field);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
