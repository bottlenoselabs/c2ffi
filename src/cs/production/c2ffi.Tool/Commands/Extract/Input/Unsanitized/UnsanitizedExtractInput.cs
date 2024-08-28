// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using bottlenoselabs.Common.Tools;
using JetBrains.Annotations;

namespace c2ffi.Tool.Commands.Extract.Input.Unsanitized;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
[PublicAPI]
public sealed class UnsanitizedExtractInput : ToolUnsanitizedInput
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
    ///     Gets or sets the object-like macros to use when parsing C code.
    /// </summary>
    [JsonPropertyName("defines")]
    public ImmutableDictionary<string, string>? Defines { get; set; }

    /// <summary>
    ///     Gets or sets the directories to ignore header files.
    /// </summary>
    [JsonPropertyName("ignoredIncludeFiles")]
    public ImmutableArray<string>? IgnoredIncludeFiles { get; set; }

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
    ///     Gets or sets a value that determines whether the C code is parsed as a single header or multiple headers.
    /// </summary>
    /// <para>
    ///     Default is <c>false</c>. Use <c>true</c> to parse the C code as a single header. Use <c>false</c> to parse
    ///     the C code as multiple headers.
    /// </para>
    [JsonPropertyName("isSingleHeader")]
    public bool? IsSingleHeader { get; set; }

    /// <summary>
    ///     Gets or sets the cursor names to be treated as opaque types.
    /// </summary>
    [JsonPropertyName("opaqueTypeNames")]
    public ImmutableArray<string>? OpaqueTypeNames { get; set; }

    /// <summary>
    ///     Gets or sets the name of macro objects that are not allowed. Use <c>null</c> to allow all macro objects.
    /// </summary>
    [JsonPropertyName("ignoredMacroObjects")]
    public ImmutableArray<string>? IgnoredMacroObjects { get; set; }

    /// <summary>
    ///     Gets or sets the name of variables that are not allowed. Use <c>null</c> to allow all variables.
    /// </summary>
    [JsonPropertyName("ignoredVariables")]
    public ImmutableArray<string>? IgnoredVariables { get; set; }

    /// <summary>
    ///     Gets or sets the name of functions that are not allowed. Use <c>null</c> to allow all functions.
    /// </summary>
    [JsonPropertyName("ignoredFunctions")]
    public ImmutableArray<string>? IgnoredFunctions { get; set; }

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

    /// <summary>
    ///     Gets or sets the name of enums that are explicitly allowed.
    /// </summary>
    [JsonPropertyName("includedNames")]
    public ImmutableArray<string>? IncludedNames { get; set; }
}
