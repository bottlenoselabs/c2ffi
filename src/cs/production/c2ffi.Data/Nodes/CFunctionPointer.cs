// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a function pointer in a C foreign function interface.
/// </summary>
[PublicAPI]
public class CFunctionPointer : CNodeWithLocation
{
    /// <summary>
    ///     Gets or sets the type information.
    /// </summary>
    [JsonPropertyName("type")]
    public CTypeInfo TypeInfo { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the return type information.
    /// </summary>
    [JsonPropertyName("return_type")]
    public CTypeInfo ReturnTypeInfo { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the parameters.
    /// </summary>
    [JsonPropertyName("parameters")]
    public ImmutableArray<CFunctionPointerParameter> Parameters { get; set; } =
        ImmutableArray<CFunctionPointerParameter>.Empty;

    /// <summary>
    ///     Gets or sets the function pointer's calling convention.
    /// </summary>
    public CFunctionCallingConvention CallingConvention { get; set; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"FunctionPointer {TypeInfo} @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CFunctionPointer other2)
        {
            return false;
        }

        return TypeInfo.Equals(other2.TypeInfo) &&
               ReturnTypeInfo.Equals(other2.ReturnTypeInfo) &&
               Parameters.SequenceEqual(other2.Parameters);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(TypeInfo);
        hashCode.Add(ReturnTypeInfo);

        foreach (var parameter in Parameters)
        {
            hashCode.Add(parameter);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
