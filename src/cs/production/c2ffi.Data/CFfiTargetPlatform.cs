// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Data;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a platform-specific foreign function interface of a C library.
/// </summary>
[PublicAPI]
public record CFfiTargetPlatform
{
    /// <summary>
    ///     Gets or sets the file name of the C header file.
    /// </summary>
    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the target platform requested to be used by Clang.
    /// </summary>
    [JsonPropertyName("platform_requested")]
    public TargetPlatform PlatformRequested { get; set; } = TargetPlatform.Unknown;

    /// <summary>
    ///     Gets or sets the target platform used by Clang.
    /// </summary>
    [JsonPropertyName("platform_actual")]
    public TargetPlatform PlatformActual { get; set; } = TargetPlatform.Unknown;

    /// <summary>
    ///     Gets or sets the arguments passed to Clang.
    /// </summary>
    [JsonPropertyName("clang_arguments")]
    public ImmutableArray<string> ClangArguments;

    /// <summary>
    ///     Gets or sets the system include directories used.
    /// </summary>
    [JsonPropertyName("system_include_directories")]
    public ImmutableArray<string> SystemIncludeDirectories { get; set; } = ImmutableArray<string>.Empty;

    /// <summary>
    ///     Gets or sets the user include directories used.
    /// </summary>
    [JsonPropertyName("user_include_directories")]
    public ImmutableArray<string> UserIncludeDirectories { get; set; } = ImmutableArray<string>.Empty;

    /// <summary>
    ///     Gets or sets the pointer size in bytes.
    /// </summary>
    [JsonPropertyName("pointer_size")]
    public int PointerSize { get; set; }

    /// <summary>
    ///     Gets or sets the macro objects.
    /// </summary>
    [JsonPropertyName("macro_objects")]
    public ImmutableSortedDictionary<string, CMacroObject> MacroObjects { get; set; } =
        ImmutableSortedDictionary<string, CMacroObject>.Empty;

    /// <summary>
    ///     Gets or sets the variables.
    /// </summary>
    [JsonPropertyName("variables")]
    public ImmutableSortedDictionary<string, CVariable> Variables { get; set; } =
        ImmutableSortedDictionary<string, CVariable>.Empty;

    /// <summary>
    ///     Gets or sets the functions.
    /// </summary>
    [JsonPropertyName("functions")]
    public ImmutableSortedDictionary<string, CFunction> Functions { get; set; } =
        ImmutableSortedDictionary<string, CFunction>.Empty;

    /// <summary>
    ///     Gets or sets the records.
    /// </summary>
    [JsonPropertyName("records")]
    public ImmutableSortedDictionary<string, CRecord> Records { get; set; } =
        ImmutableSortedDictionary<string, CRecord>.Empty;

    /// <summary>
    ///     Gets or sets the enums.
    /// </summary>
    [JsonPropertyName("enums")]
    public ImmutableSortedDictionary<string, CEnum> Enums { get; set; } =
        ImmutableSortedDictionary<string, CEnum>.Empty;

    /// <summary>
    ///     Gets or sets the type aliases.
    /// </summary>
    [JsonPropertyName("aliases")]
    public ImmutableSortedDictionary<string, CTypeAlias> TypeAliases { get; set; } =
        ImmutableSortedDictionary<string, CTypeAlias>.Empty;

    /// <summary>
    ///     Gets or sets the opaque types.
    /// </summary>
    [JsonPropertyName("opaques")]
    public ImmutableSortedDictionary<string, COpaqueType> OpaqueTypes { get; set; } =
        ImmutableSortedDictionary<string, COpaqueType>.Empty;

    /// <summary>
    ///     Gets or sets the function pointers.
    /// </summary>
    [JsonPropertyName("function_pointers")]
    public ImmutableSortedDictionary<string, CFunctionPointer> FunctionPointers { get; set; } =
        ImmutableSortedDictionary<string, CFunctionPointer>.Empty;
}
