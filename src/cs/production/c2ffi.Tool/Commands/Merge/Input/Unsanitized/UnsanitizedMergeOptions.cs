// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace c2ffi.Tool.Commands.Merge.Input.Unsanitized;

// NOTE: This class is considered un-sanitized input; all strings and other types could be null.
public class UnsanitizedMergeOptions
{
    public string InputDirectoryPath { get; set; } = string.Empty;

    public string OutputFilePath { get; set; } = string.Empty;
}
