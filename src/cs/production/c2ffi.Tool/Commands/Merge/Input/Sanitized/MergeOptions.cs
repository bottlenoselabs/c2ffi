// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;

namespace c2ffi.Tool.Commands.Merge.Input.Sanitized;

public class MergeOptions
{
    public ImmutableArray<string> InputFilePaths { get; set; } = ImmutableArray<string>.Empty;

    public string OutputFilePath { get; set; } = string.Empty;
}
