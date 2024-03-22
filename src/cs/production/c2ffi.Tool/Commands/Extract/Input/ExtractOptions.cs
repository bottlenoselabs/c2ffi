// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;

namespace c2ffi.Tool.Commands.Extract.Input;

public sealed class ExtractOptions
{
    public string InputFilePath { get; init; } = string.Empty;

    public ImmutableArray<ExtractTargetPlatformOptions> TargetPlatformsOptions { get; init; }
}
