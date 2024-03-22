// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace c2ffi.Tests.Library.Helpers;

[ExcludeFromCodeCoverage]
public class FileSystemHelper
{
    private readonly IFileSystem _fileSystem;
    private string? _gitRepositoryRootDirectoryPath;

    public string GitRepositoryRootDirectoryPath => _gitRepositoryRootDirectoryPath ??= FindGitRepositoryRootDirectoryPath();

    public FileSystemHelper(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    private string FindGitRepositoryRootDirectoryPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var directoryInfo = _fileSystem.DirectoryInfo.New(baseDirectory);
        while (true)
        {
            var files = directoryInfo.GetFiles(".gitignore", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return directoryInfo.FullName;
            }

            directoryInfo = directoryInfo.Parent;
            if (directoryInfo == null)
            {
                return string.Empty;
            }
        }
    }
}
