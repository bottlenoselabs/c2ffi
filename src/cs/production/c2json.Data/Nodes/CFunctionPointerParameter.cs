// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2json.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a function pointer parameter in a C foreign function interface.
/// </summary>
[PublicAPI]
public class CFunctionPointerParameter : CNode
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    [JsonPropertyName("name")]
    public new string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    /// <summary>
    ///     Gets or sets the type information.
    /// </summary>
    [JsonPropertyName("type")]
    public CTypeInfo TypeInfo { get; set; } = null!;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"FunctionPointerParameter '{Name}': {TypeInfo}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CFunctionPointerParameter other2)
        {
            return false;
        }

        return TypeInfo.Equals(other2.TypeInfo);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(TypeInfo);

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
