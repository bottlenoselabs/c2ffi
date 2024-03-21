// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace c2json.Tests;

public abstract class TestBase
{
    private readonly string _relativeDataFilesDirectory;
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly bool _regenerateDataFiles;
    private readonly string _sourceDirectoryPath;

    protected TestBase(string baseDataFilesDirectory, bool regenerateDataFiles = false)
    {
        var services = TestHost.Services;
        _fileSystem = services.GetService<IFileSystem>()!;
        var path = _fileSystem.Path;
        _sourceDirectoryPath = GetSourceDirectoryPath(_fileSystem);
        _relativeDataFilesDirectory = path.Combine(_sourceDirectoryPath, baseDataFilesDirectory);

        _regenerateDataFiles = regenerateDataFiles;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    protected void AssertValue<T>(string name, T value, string directory)
    {
        var relativeJsonFilePath =
            _fileSystem.Path.Combine(_relativeDataFilesDirectory, directory, $"{name}.json");
        var jsonFilePath = _fileSystem.Path.Combine(_sourceDirectoryPath, relativeJsonFilePath);
        if (_regenerateDataFiles)
        {
            RegenerateDataFile(jsonFilePath, value);
        }

        var expectedValue = ReadValueFromFile<T>(jsonFilePath);
        value.Should().BeEquivalentTo(
            expectedValue,
            o => o.ComparingByMembers<T>(),
            $"because that is what the JSON file has `{jsonFilePath}`");
    }

    private void RegenerateDataFile<T>(string filePath, T value)
    {
        WriteValueToFile(filePath, value);
    }

    private T? ReadValueFromFile<T>(string filePath)
    {
        var fileContents = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(fileContents, _jsonSerializerOptions);
    }

    private void WriteValueToFile<T>(string filePath, T value)
    {
        var fullFilePath = _fileSystem.Path.GetFullPath(filePath);

        var outputDirectory = _fileSystem.Path.GetDirectoryName(fullFilePath)!;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = AppContext.BaseDirectory;
            fullFilePath = Path.Combine(Environment.CurrentDirectory, fullFilePath);
        }

        if (!_fileSystem.Directory.Exists(outputDirectory))
        {
            _fileSystem.Directory.CreateDirectory(outputDirectory);
        }

        if (_fileSystem.File.Exists(fullFilePath))
        {
            _fileSystem.File.Delete(fullFilePath);
        }

        var fileContents = JsonSerializer.Serialize(value, _jsonSerializerOptions);

        using var fileStream = _fileSystem.File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    private static string GetGitRepositoryDirectoryPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(baseDirectory);
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

    private string GetSourceDirectoryPath(IFileSystem fileSystem)
    {
        var path = fileSystem.Path.Combine(
            GetGitRepositoryDirectoryPath(),
            "src",
            "cs",
            "tests",
            "c2json.Tests");
        if (!fileSystem.Directory.Exists(path))
        {
            throw new InvalidOperationException("The tests source directory does not exist.");
        }

        return path;
    }
}
