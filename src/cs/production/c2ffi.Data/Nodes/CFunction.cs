// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a function in a C foreign function interface.
/// </summary>
[PublicAPI]
public class CFunction : CNodeWithLocation
{
    /// <summary>
    ///     Gets or sets the function's calling convention.
    /// </summary>
    [JsonPropertyName("calling_convention")]
    public CFunctionCallingConvention CallingConvention { get; set; }

    /// <summary>
    ///     Gets or sets the function's return type information.
    /// </summary>
    [JsonPropertyName("return_type")]
    public CType ReturnType { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the function's parameters.
    /// </summary>
    [JsonPropertyName("parameters")]
    public ImmutableArray<CFunctionParameter> Parameters { get; set; } = ImmutableArray<CFunctionParameter>.Empty;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"Function '{Name}' @ {Location}";
    }

    /// <inheritdoc />
    public override bool Equals(CNode? other)
    {
        if (!base.Equals(other) || other is not CFunction other2)
        {
            return false;
        }

        var callingConventionsAreEqual = CallingConvention == other2.CallingConvention;
        var parametersAreEqual = Parameters.SequenceEqual(other2.Parameters);
        var returnTypesAreEqual = ReturnType.Equals(other2.ReturnType);
        return callingConventionsAreEqual && returnTypesAreEqual && parametersAreEqual;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var baseHashCode = base.GetHashCode();

        var hashCode = default(HashCode);
        hashCode.Add(baseHashCode);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        hashCode.Add(CallingConvention);
        hashCode.Add(ReturnType);

        foreach (var parameter in Parameters)
        {
            hashCode.Add(parameter);
        }

        // ReSharper restore NonReadonlyMemberInGetHashCode

        return hashCode.ToHashCode();
    }
}
