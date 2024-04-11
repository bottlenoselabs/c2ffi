// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using c2ffi.Tool.Commands.Merge.Input.Sanitized;
using c2ffi.Tool.Commands.Merge.Input.Unsanitized;

namespace c2ffi.Tool.Commands.Merge.Input;

public sealed class MergeInputSanitizer : ToolInputSanitizer<UnsanitizedMergeInput, MergeInput>
{
    private readonly IFileSystem _fileSystem;

    public MergeInputSanitizer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public override MergeInput Sanitize(UnsanitizedMergeInput unsanitizedInput)
    {
        var directoryPath = _fileSystem.Path.GetFullPath(unsanitizedInput.InputDirectoryPath);
        if (!_fileSystem.Directory.Exists(directoryPath))
        {
            throw new ToolInputSanitizationException($"The directory '{directoryPath}' does not exist.");
        }

        var filePaths = _fileSystem.Directory.GetFiles(directoryPath, "*.json").ToImmutableArray();

        if (filePaths.IsDefaultOrEmpty)
        {
            throw new ToolInputSanitizationException($"The directory '{directoryPath}' does not contain any abstract syntax tree `.json` files.");
        }

        var outputFilePath = _fileSystem.Path.GetFullPath(unsanitizedInput.OutputFilePath);

        var result = new MergeInput
        {
            OutputFilePath = outputFilePath,
            InputFilePaths = filePaths
        };

        return result;
    }
}
