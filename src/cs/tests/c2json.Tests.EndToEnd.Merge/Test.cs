// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using c2json.Tests.Library;
using c2json.Tests.Library.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace c2json.Tests.EndToEnd.Merge;

public class Test
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFileSystem _fileSystem;
    private readonly FileSystemHelper _fileSystemHelper;

    public Test(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var services = TestHost.Services;
        _fileSystem = services.GetService<IFileSystem>()!;
        _fileSystemHelper = services.GetService<FileSystemHelper>()!;
    }

    [Fact]
    public void Exists()
    {
        var rootDirectoryPath = _fileSystemHelper.GitRepositoryRootDirectoryPath;
        var x = _fileSystem.Path.Combine(rootDirectoryPath, "src/c/tests/functions/function_int/ast");
        var y = _fileSystem.DirectoryInfo.New(x);
        Assert.True(y.Exists);
        foreach (var z in y.EnumerateFiles())
        {
            _testOutputHelper.WriteLine(z.FullName);
        }
    }
}
