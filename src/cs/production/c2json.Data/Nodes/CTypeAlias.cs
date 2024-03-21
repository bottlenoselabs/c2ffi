// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2json.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a typedef alias in C abstract syntax tree.
/// </summary>
[PublicAPI]
public class CTypeAlias : CNodeWithLocation
{
    /// <summary>
    ///     Gets or sets the underlying type information.
    /// </summary>
    [JsonPropertyName("underlyingType")]
    public CTypeInfo UnderlyingTypeInfo { get; set; } = null!;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Typedef '{Name}': {UnderlyingTypeInfo} @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CTypeAlias other2)
        {
            return false;
        }

        return UnderlyingTypeInfo.Equals(other2.UnderlyingTypeInfo);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(UnderlyingTypeInfo);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
