// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Data;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a cross-platform C foreign function interface.
/// </summary>
[PublicAPI]
public record CFfiCrossPlatform
{
    /// <summary>
    ///     Gets or sets the target platforms used.
    /// </summary>
    [JsonPropertyName("platforms")]
    public ImmutableArray<TargetPlatform> Platforms { get; set; } = ImmutableArray<TargetPlatform>.Empty;

    /// <summary>
    ///     Gets or sets the macro objects.
    /// </summary>
    [JsonPropertyName("macroObjects")]
    public ImmutableDictionary<string, CMacroObject> MacroObjects { get; set; } =
        ImmutableDictionary<string, CMacroObject>.Empty;

    /// <summary>
    ///     Gets or sets the variables.
    /// </summary>
    [JsonPropertyName("variables")]
    public ImmutableDictionary<string, CVariable> Variables { get; set; } =
        ImmutableDictionary<string, CVariable>.Empty;

    /// <summary>
    ///     Gets or sets the functions..
    /// </summary>
    [JsonPropertyName("functions")]
    public ImmutableDictionary<string, CFunction> Functions { get; set; } =
        ImmutableDictionary<string, CFunction>.Empty;

    /// <summary>
    ///     Gets or sets the records.
    /// </summary>
    [JsonPropertyName("records")]
    public ImmutableDictionary<string, CRecord> Records { get; set; } = ImmutableDictionary<string, CRecord>.Empty;

    /// <summary>
    ///     Gets or sets the enums.
    /// </summary>
    [JsonPropertyName("enums")]
    public ImmutableDictionary<string, CEnum> Enums { get; set; } = ImmutableDictionary<string, CEnum>.Empty;

    /// <summary>
    ///     Gets or sets the enum constants.
    /// </summary>
    [JsonPropertyName("enumConstants")]
    public ImmutableDictionary<string, CEnumConstant> EnumConstants { get; set; } =
        ImmutableDictionary<string, CEnumConstant>.Empty;

    /// <summary>
    ///     Gets or sets the type aliases.
    /// </summary>
    [JsonPropertyName("typeAliases")]
    public ImmutableDictionary<string, CTypeAlias> TypeAliases { get; set; } =
        ImmutableDictionary<string, CTypeAlias>.Empty;

    /// <summary>
    ///     Gets or sets the opaque types.
    /// </summary>
    [JsonPropertyName("opaqueTypes")]
    public ImmutableDictionary<string, COpaqueType> OpaqueTypes { get; set; } =
        ImmutableDictionary<string, COpaqueType>.Empty;

    /// <summary>
    ///     Gets or sets the function pointers.
    /// </summary>
    [JsonPropertyName("functionPointers")]
    public ImmutableDictionary<string, CFunctionPointer> FunctionPointers { get; set; } =
        ImmutableDictionary<string, CFunctionPointer>.Empty;
}
