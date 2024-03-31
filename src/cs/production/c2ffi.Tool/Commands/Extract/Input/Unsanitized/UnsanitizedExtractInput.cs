// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Tool.Commands.Extract.Input.Unsanitized;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
[PublicAPI]
public sealed class UnsanitizedExtractInput
{
    /// <summary>
    ///     Gets or sets the path of the output FFI directory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The directory will contain one or more generated FFI`.json` files which each have a
    ///         file name of the target platform.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("outputDirectory")]
    public string? OutputDirectory { get; set; } = "./ffi";

    /// <summary>
    ///     Gets or sets the path of the input `.h` header file containing C code.
    /// </summary>
    [JsonPropertyName("inputFilePath")]
    public string? InputFilePath { get; set; }

    /// <summary>
    ///     Gets or sets the directories to search for non-system header files.
    /// </summary>
    [JsonPropertyName("userIncludeDirectories")]
    public ImmutableArray<string>? UserIncludeDirectories { get; set; }

    /// <summary>
    ///     Gets or sets the directories to search for system header files.
    /// </summary>
    [JsonPropertyName("systemIncludeDirectories")]
    public ImmutableArray<string>? SystemIncludeDirectories { get; set; }

    /// <summary>
    ///     Gets or sets the directories to ignore header files.
    /// </summary>
    [JsonPropertyName("ignoreIncludeFiles")]
    public ImmutableArray<string>? IgnoredIncludeFiles { get; set; }

    /// <summary>
    ///     Gets or sets a value that determines whether to show the the path of header code locations with full paths
    ///     or relative paths.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>false</c>. Use <c>true</c> to use the full path for header locations. Use <c>false</c> to
    ///         show only relative file paths.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledLocationFullPaths")]
    public bool? IsEnabledLocationFullPaths { get; set; }

    /// <summary>
    ///     Gets or sets a value that determines whether to include or exclude declarations (functions, enums, structs,
    ///     typedefs, etc) with a prefixed underscore that are assumed to be 'non public'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>false</c>. Use <c>true</c> to include declarations with a prefixed underscore. Use
    ///         <c>false</c> to exclude declarations with a prefixed underscore.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledAllowNamesWithPrefixedUnderscore")]
    public bool? IsEnabledAllowNamesWithPrefixedUnderscore { get; set; }

    /// <summary>
    ///     Gets or sets a value that determines whether to include or exclude system declarations (functions, enums,
    ///     typedefs, records, etc).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is `false`. Use <c>true</c> to include system declarations. Use `false` to exclude system
    ///         declarations.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledSystemDeclarations")]
    public bool? IsEnabledSystemDeclarations { get; set; }

    /// <summary>
    ///     Gets or sets a value that determines whether to automatically find and append the system headers for the
    ///     target platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default is <c>true</c>. Use <c>true</c> to automatically find and append system headers for the target
    ///         platform. Use <c>false</c> to skip.
    ///     </para>
    /// </remarks>
    [JsonPropertyName("isEnabledAutomaticallyFindSystemHeaders")]
    public bool? IsEnabledAutomaticallyFindSystemHeaders { get; set; }

    /// <summary>
    ///     Gets or sets determines whether to parse only the top-level cursors which are externally visible, or all
    ///     top-level cursors.
    /// </summary>
    /// <para>
    ///     Default is <c>true</c>. Use <c>true</c> to parse only top-level cursors which are externally visible. Use
    ///     <c>false</c> to parse all top-level cursors whether or not they are externally visible.
    /// </para>
    [JsonPropertyName("isEnabledOnlyExternalTopLevelCursors")]
    public bool? IsEnabledOnlyExternalTopLevelCursors { get; set; }

    /// <summary>
    ///     Gets or sets the cursor names to be treated as opaque types.
    /// </summary>
    [JsonPropertyName("opaqueTypeNames")]
    public ImmutableArray<string>? OpaqueTypeNames { get; set; }

    /// <summary>
    ///     Gets or sets the name of macro objects allowed. Use <c>null</c> to allow all macro objects.
    /// </summary>
    [JsonPropertyName("allowedMacroObjects")]
    public ImmutableArray<string>? AllowedMacroObjects { get; set; }

    /// <summary>
    ///     Gets or sets the name of variables allowed. Use <c>null</c> to allow all variables.
    /// </summary>
    [JsonPropertyName("allowedVariables")]
    public ImmutableArray<string>? AllowedVariables { get; set; }

    /// <summary>
    ///     Gets or sets the target platform configurations for extracting the FFIs per desktop host
    ///     operating system.
    /// </summary>
    [JsonPropertyName("targetPlatforms")]
    public ImmutableDictionary<string, ImmutableDictionary<string, UnsanitizedExtractInputTargetPlatform>>? TargetPlatforms { get; set; }

    /// <summary>
    ///     Gets or sets the names of libraries and/or interfaces for macOS, iOS, tvOS or watchOS.
    /// </summary>
    [JsonPropertyName("appleFrameworks")]
    public ImmutableArray<string>? AppleFrameworks { get; set; }
}
