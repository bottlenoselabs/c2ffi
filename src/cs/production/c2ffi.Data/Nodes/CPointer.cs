// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a pointer in a C foreign function interface.
/// </summary>
[PublicAPI]
public class CPointer : CNode
{
    /// <summary>
    ///     Gets or sets the type information.
    /// </summary>
    [JsonPropertyName("type")]
    public CType Type { get; set; } = null!;

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CPointer other2)
        {
            return false;
        }

        return Type.Equals(other2.Type);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(Type);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
