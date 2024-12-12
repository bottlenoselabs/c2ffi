// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using JetBrains.Annotations;

namespace c2ffi.Merge;

[UsedImplicitly]
public sealed class InputSanitizer(IFileSystem fileSystem) : ToolInputSanitizer<InputUnsanitized, InputSanitized>
{
    public override InputSanitized Sanitize(InputUnsanitized inputUnsanitizedInput)
    {
        var directoryPath = fileSystem.Path.GetFullPath(inputUnsanitizedInput.InputDirectoryPath);
        if (!fileSystem.Directory.Exists(directoryPath))
        {
            throw new ToolInputSanitizationException($"The directory '{directoryPath}' does not exist.");
        }

        var filePaths = fileSystem.Directory.GetFiles(directoryPath, "*.json").ToImmutableArray();

        if (filePaths.IsDefaultOrEmpty)
        {
            throw new ToolInputSanitizationException($"The directory '{directoryPath}' does not contain any abstract syntax tree `.json` files.");
        }

        var outputFilePath = fileSystem.Path.GetFullPath(inputUnsanitizedInput.OutputFilePath);

        var result = new InputSanitized
        {
            OutputFilePath = outputFilePath,
            InputFilePaths = filePaths
        };

        return result;
    }
}
