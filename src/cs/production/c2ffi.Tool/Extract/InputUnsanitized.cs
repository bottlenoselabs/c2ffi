// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using bottlenoselabs.Common.Tools;
using JetBrains.Annotations;

namespace c2ffi.Extract;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
[PublicAPI]
[UsedImplicitly]
public sealed class InputUnsanitized : ToolUnsanitizedInput
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
    ///     Gets or sets the Clang cursor names to be treated as opaque types.
    /// </summary>
    [JsonPropertyName("opaqueTypeNames")]
    public ImmutableArray<string>? OpaqueTypeNames { get; set; }

    /// <summary>
    ///     Gets or sets the name of Clang cursors / Clang types that are not allowed.
    /// </summary>
    [JsonPropertyName("ignoredNames")]
    public ImmutableArray<string>? IgnoredNames { get; set; }

    /// <summary>
    ///     Gets or sets the name of Clang cursors / Clang types that are explicitly allowed.
    /// </summary>
    [JsonPropertyName("includedNames")]
    public ImmutableArray<string>? IncludedNames { get; set; }

    /// <summary>
    ///     Gets or sets the target platform configurations for extracting the FFIs per desktop host
    ///     operating system.
    /// </summary>
    [JsonPropertyName("targetPlatforms")]
    public ImmutableDictionary<string, ImmutableDictionary<string, InputUnsanitizedTargetPlatform>>? TargetPlatforms { get; set; }
}
