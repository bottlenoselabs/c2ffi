// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents an opaque type in a C foreign function interface.
/// </summary>
[PublicAPI]
public class COpaqueType : CNodeWithLocation
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"OpaqueType '{Name}' @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        return base.Equals(other) && other is COpaqueType;
    }
}
