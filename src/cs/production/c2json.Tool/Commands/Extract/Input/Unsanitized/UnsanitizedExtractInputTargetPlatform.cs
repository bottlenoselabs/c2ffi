// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2json.Tool.Commands.Extract.Input.Unsanitized;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
[PublicAPI]
public sealed class UnsanitizedExtractInputTargetPlatform
{
    /// <summary>
    ///     Gets or sets the directories to search for non-system header files specific to the target platform.
    /// </summary>
    [JsonPropertyName("userIncludeDirectories")]
    public ImmutableArray<string>? UserIncludeDirectories { get; set; }

    /// <summary>
    ///     Gets or sets the directories to search for system header files of the target platform.
    /// </summary>
    [JsonPropertyName("systemIncludeDirectories")]
    public ImmutableArray<string>? SystemIncludeDirectories { get; set; }

    /// <summary>
    ///     Gets or sets the directories to ignore header files for either user or system.
    /// </summary>
    [JsonPropertyName("ignoredIncludeDirectories")]
    public ImmutableArray<string>? IgnoredIncludeDirectories { get; set; }

    /// <summary>
    ///     Gets or sets the object-like macros to use when parsing C code.
    /// </summary>
    [JsonPropertyName("defines")]
    public ImmutableArray<string>? Defines { get; set; }

    /// <summary>
    ///     Gets or sets the additional Clang arguments to use when parsing C code.
    /// </summary>
    [JsonPropertyName("clangArguments")]
    public ImmutableArray<string>? ClangArguments { get; set; }

    /// <summary>
    ///     Gets or sets the names of libraries and/or interfaces for macOS, iOS, tvOS or watchOS.
    /// </summary>
    [JsonPropertyName("appleFrameworks")]
    public ImmutableArray<string>? AppleFrameworks { get; set; }
}
