// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using c2ffi.Tool.Commands.Merge.Input.Sanitized;
using c2ffi.Tool.Commands.Merge.Input.Unsanitized;
using JetBrains.Annotations;

namespace c2ffi.Tool.Commands.Merge.Input;

[UsedImplicitly]
public sealed class MergeInputSanitizer(IFileSystem fileSystem) : ToolInputSanitizer<UnsanitizedMergeInput, MergeInput>
{
    public override MergeInput Sanitize(UnsanitizedMergeInput unsanitizedInput)
    {
        var directoryPath = fileSystem.Path.GetFullPath(unsanitizedInput.InputDirectoryPath);
        if (!fileSystem.Directory.Exists(directoryPath))
        {
            throw new ToolInputSanitizationException($"The directory '{directoryPath}' does not exist.");
        }

        var filePaths = fileSystem.Directory.GetFiles(directoryPath, "*.json").ToImmutableArray();

        if (filePaths.IsDefaultOrEmpty)
        {
            throw new ToolInputSanitizationException($"The directory '{directoryPath}' does not contain any abstract syntax tree `.json` files.");
        }

        var outputFilePath = fileSystem.Path.GetFullPath(unsanitizedInput.OutputFilePath);

        var result = new MergeInput
        {
            OutputFilePath = outputFilePath,
            InputFilePaths = filePaths
        };

        return result;
    }
}
