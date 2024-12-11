// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;
using bottlenoselabs.Common.Tools;

namespace c2ffi.Tool.Internal.Input;

#pragma warning disable CA1515
public abstract class InputSanitizer<TUnsanitizedInput, TSanitizedInput> : ToolInputSanitizer<TUnsanitizedInput, TSanitizedInput>
#pragma warning restore CA1515
    where TUnsanitizedInput : class
    where TSanitizedInput : class
{
    protected readonly IPath Path;
    protected readonly IFile File;
    protected readonly IDirectory Directory;

    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    protected InputSanitizer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        Path = _fileSystem.Path;
        File = _fileSystem.File;
        Directory = _fileSystem.Directory;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        };
    }

    public TSanitizedInput SanitizeFromFile(string filePath)
    {
        var fullFilePath = Path.GetFullPath(filePath);
        if (!_fileSystem.File.Exists(fullFilePath))
        {
            throw new InputSanitizationException($"The file '{fullFilePath}' does not exist.");
        }

        var fileContents = _fileSystem.File.ReadAllText(fullFilePath);
        if (string.IsNullOrEmpty(fileContents))
        {
            throw new InputSanitizationException($"The file '{fullFilePath}' is empty.");
        }

        var unsanitizedInput = JsonSerializer.Deserialize<TUnsanitizedInput>(fileContents, _jsonSerializerOptions) ??
                               throw new InputSanitizationException($"The {typeof(TUnsanitizedInput)} is null.");
        var previousCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.GetDirectoryName(fullFilePath)!;
        var result = Sanitize(unsanitizedInput);
        Environment.CurrentDirectory = previousCurrentDirectory;

        return result;
    }

    protected ImmutableArray<string> SanitizeStrings(ImmutableArray<string>? strings)
    {
        if (strings == null || strings.Value.IsDefaultOrEmpty)
        {
            return ImmutableArray<string>.Empty;
        }

        var result = strings.Value
            .Where(x => !string.IsNullOrEmpty(x)).ToImmutableArray();
        return result;
    }

    protected ImmutableArray<string> SanitizeStringsAndCombine(
        ImmutableArray<string>? strings1,
        ImmutableArray<string>? strings2)
    {
        var sanitizedStrings1 = SanitizeStrings(strings1);
        var sanitizedStrings2 = SanitizeStrings(strings2);
        var result = sanitizedStrings1.AddRange(sanitizedStrings2);
        return result;
    }

    protected ImmutableDictionary<string, string> SanitizeStrings(ImmutableDictionary<string, string>? strings)
    {
        if (strings == null || strings.IsEmpty)
        {
            return ImmutableDictionary<string, string>.Empty;
        }

        var result = strings.Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value)).ToImmutableDictionary();
        return result;
    }

    protected ImmutableDictionary<string, string> SanitizeStringsAndCombine(
        ImmutableDictionary<string, string>? strings1,
        ImmutableDictionary<string, string>? strings2)
    {
        var sanitizedStrings1 = SanitizeStrings(strings1);
        var sanitizedStrings2 = SanitizeStrings(strings2);
        var result = sanitizedStrings1.AddRange(sanitizedStrings2);
        return result;
    }

    protected ImmutableArray<string> SanitizeDirectoryPaths(
        ImmutableArray<string>? directoryPaths1,
        ImmutableArray<string>? directoryPaths2 = null)
    {
        var directoryPaths = SanitizeStringsAndCombine(directoryPaths1, directoryPaths2);
        var builder = ImmutableArray.CreateBuilder<string>();
        foreach (var directoryPath in directoryPaths)
        {
            string fullDirectoryPath;
            try
            {
                fullDirectoryPath = Path.GetFullPath(directoryPath);
            }
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                continue;
            }

            builder.Add(fullDirectoryPath);
        }

        return builder.ToImmutable();
    }

    protected ImmutableArray<Regex> SanitizeRegexes(ImmutableArray<string>? unsanitizedStringPatterns)
    {
        var stringPatterns = SanitizeStrings(unsanitizedStringPatterns);
        var regexes = stringPatterns
            .Select(stringPattern => new Regex(stringPattern))
            .ToImmutableArray();
        return regexes;
    }
}
