// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents information about a C type.
/// </summary>
[PublicAPI]
public class CTypeInfo : IEquatable<CTypeInfo>
{
    /// <summary>
    ///     Gets or sets the name of the C type.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the kind of C type.
    /// </summary>
    [JsonPropertyName("kind")]
    public CNodeKind NodeKind { get; set; } = CNodeKind.Unknown;

    /// <summary>
    ///     Gets or sets the byte size of the C type.
    /// </summary>
    [JsonPropertyName("size_of")]
    public int? SizeOf { get; set; }

    /// <summary>
    ///     Gets or sets the byte alignment of the C type.
    /// </summary>
    [JsonPropertyName("align_of")]
    public int? AlignOf { get; set; }

    /// <summary>
    ///     Gets or sets the byte size of the element for array and pointer types.
    /// </summary>
    [JsonPropertyName("size_of_element")]
    public int? ElementSize { get; set; }

    /// <summary>
    ///     Gets or sets the element size of the array for array types.
    /// </summary>
    [JsonPropertyName("array_size")]
    public int? ArraySizeOf { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the C type is anonymous.
    /// </summary>
    [JsonPropertyName("is_snonymous")]
    public bool? IsAnonymous { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the C type is read-only.
    /// </summary>
    [JsonPropertyName("is_const")]
    public bool IsConst { get; set; }

    /// <summary>
    ///     Gets or sets the cursor location of the C type definition in the original C header.
    /// </summary>
    [JsonPropertyName("location")]
    public CLocation? Location { get; set; }

    /// <summary>
    ///     Gets or sets the inner type information for pointer, array, and typedef alias types.
    /// </summary>
    [JsonPropertyName("inner_type")]
    public CTypeInfo? InnerTypeInfo { get; set; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Determines whether the specified <see cref="CTypeInfo" /> is equal to the current <see cref="CTypeInfo" />.
    /// </summary>
    /// <param name="other">The <see cref="CTypeInfo" /> to compare with the current <see cref="CTypeInfo" />.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified <see cref="CTypeInfo" /> is equal to the current
    ///     <see cref="CTypeInfo" />; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(CTypeInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name &&
               NodeKind == other.NodeKind &&
               SizeOf == other.SizeOf &&
               AlignOf == other.AlignOf &&
               ElementSize == other.ElementSize &&
               ArraySizeOf == other.ArraySizeOf &&
               IsAnonymous == other.IsAnonymous &&
               IsConst == other.IsConst;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CTypeInfo)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(Name);
        hashCode.Add((int)NodeKind);
        hashCode.Add(SizeOf);
        hashCode.Add(AlignOf);
        hashCode.Add(ElementSize);
        hashCode.Add(ArraySizeOf);
        hashCode.Add(IsAnonymous);
        hashCode.Add(IsConst);
        hashCode.Add(Location);
        hashCode.Add(InnerTypeInfo);
        return hashCode.ToHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
