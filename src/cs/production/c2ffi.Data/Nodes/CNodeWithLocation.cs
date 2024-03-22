// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents the abstract base for a node in a C foreign function interface with a defined
///     file location in the original C header.
/// </summary>
[PublicAPI]
public abstract class CNodeWithLocation : CNode
{
    /// <summary>
    ///     Gets or sets the file location in the original C header.
    /// </summary>
    [JsonPropertyName("location")]
    public CLocation? Location { get; set; }

    /// <inheritdoc />
    protected override int CompareToInternal(CNode? other)
    {
        if (other is not CNodeWithLocation other2)
        {
            return base.CompareToInternal(other);
        }

        if (Location is null && other2.Location is null)
        {
            return 0;
        }

        if (Location is null && other2.Location is not null)
        {
            return 1;
        }

        if (Location is not null && other2.Location is null)
        {
            return -1;
        }

        return Location!.Value.CompareTo(other2.Location!.Value);
    }
}
