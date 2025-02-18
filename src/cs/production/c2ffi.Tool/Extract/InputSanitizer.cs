// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using bottlenoselabs.Common.Tools;
using c2ffi.Data;
using JetBrains.Annotations;

namespace c2ffi.Extract;

[UsedImplicitly]
public sealed class InputSanitizer(IFileSystem fileSystem) : InputSanitizer<InputUnsanitized, InputSanitized>(fileSystem)
{
    private readonly string _hostOperatingSystemString = Native.OperatingSystem.ToString().ToUpperInvariant();

    public override InputSanitized Sanitize(InputUnsanitized inputUnsanitizedInput)
    {
        inputUnsanitizedInput.TargetPlatforms ??= CreateDefaultTargetPlatformsUnsanitizedInput();
        var inputFilePath = SanitizeCInputFilePath(inputUnsanitizedInput.InputFilePath);
        var targetPlatformsOptions = SanitizeTargetPlatformsOptions(inputUnsanitizedInput, inputFilePath);

        var result = new InputSanitized
        {
            InputFilePath = inputFilePath,
            TargetPlatformInputs = targetPlatformsOptions
        };

        return result;
    }

    private ImmutableDictionary<string, ImmutableDictionary<string, InputUnsanitizedTargetPlatform>> CreateDefaultTargetPlatformsUnsanitizedInput()
    {
        var extractOptionsByTargetPlatformString = new Dictionary<string, InputUnsanitizedTargetPlatform>();
        var targetPlatformString = Native.Platform.ToString();
        extractOptionsByTargetPlatformString.Add(targetPlatformString, new InputUnsanitizedTargetPlatform());

        var targetPlatformStringsByOperatingSystemString = new Dictionary<string, ImmutableDictionary<string, InputUnsanitizedTargetPlatform>>
        {
            {
                _hostOperatingSystemString, extractOptionsByTargetPlatformString.ToImmutableDictionary()
            }
        };

        var result = targetPlatformStringsByOperatingSystemString.ToImmutableDictionary();
        return result;
    }

    private string SanitizeCInputFilePath(string? inputFilePath)
    {
        if (string.IsNullOrEmpty(inputFilePath))
        {
            throw new ToolInputSanitizationException("The C input file can not be null, empty, or whitespace.");
        }

        var filePath = Path.GetFullPath(inputFilePath);

        if (!File.Exists(filePath))
        {
            throw new ToolInputSanitizationException($"The C input file does not exist: `{filePath}`.");
        }

        return filePath;
    }

    private ImmutableArray<InputSanitizedTargetPlatform> SanitizeTargetPlatformsOptions(
        InputUnsanitized inputUnsanitizedInput,
        string inputFilePath)
    {
        var builder = ImmutableArray.CreateBuilder<InputSanitizedTargetPlatform>();

        var isAtLeastOneMatchingOperatingSystem = false;
        var targetPlatformsUnsanitizedInputByOperatingSystem = inputUnsanitizedInput.TargetPlatforms;
        if (targetPlatformsUnsanitizedInputByOperatingSystem is { IsEmpty: false })
        {
            foreach (var (operatingSystemString, targetPlatformsUnsanitizedInput) in targetPlatformsUnsanitizedInputByOperatingSystem)
            {
                var isMatchingOperatingSystem =
                    operatingSystemString.Equals(_hostOperatingSystemString, StringComparison.OrdinalIgnoreCase);
                if (!isMatchingOperatingSystem)
                {
                    continue;
                }

                foreach (var (targetPlatformString, unsanitizedTargetPlatformInput) in targetPlatformsUnsanitizedInput)
                {
                    var targetPlatformOptions = SanitizeTargetPlatformInput(inputUnsanitizedInput, targetPlatformString, unsanitizedTargetPlatformInput, inputFilePath);
                    builder.Add(targetPlatformOptions);
                }

                isAtLeastOneMatchingOperatingSystem = true;
            }
        }

        if (!isAtLeastOneMatchingOperatingSystem)
        {
            throw new ToolInputSanitizationException(
                $"The current host operating system '{_hostOperatingSystemString}' was not specified in the extract options for target platforms.");
        }

        return builder.ToImmutable();
    }

    private InputSanitizedTargetPlatform SanitizeTargetPlatformInput(
        InputUnsanitized input,
        string targetPlatformString,
        InputUnsanitizedTargetPlatform targetPlatformInput,
        string inputFilePath)
    {
        var targetPlatform = new TargetPlatform(targetPlatformString);

        var allowedNameRegexes = AllowedNameRegexes(input);
        var blockedNamesRegexes = BlockedNameRegexes(input);
        var options = new InputSanitizedTargetPlatform(allowedNameRegexes, blockedNamesRegexes)
        {
            TargetPlatform = targetPlatform,
            OutputFilePath = OutputFilePath(input, targetPlatformString),
            SystemIncludeDirectories = SystemIncludeDirectories(input, targetPlatformInput),
            UserIncludeDirectories = UserIncludeDirectories(input, targetPlatformInput, inputFilePath),
            IgnoredIncludeFiles = IgnoredIncludeFiles(input, targetPlatformInput),
            MacroObjectDefines = ClangDefines(input, targetPlatformInput),
            AdditionalArguments = ClangArguments(targetPlatformInput),
            IsEnabledFindSystemHeaders = input.IsEnabledAutomaticallyFindSystemHeaders ?? true
        };

        return options;
    }

    private string OutputFilePath(InputUnsanitized input, string targetPlatformString)
    {
        return SanitizeOutputDirectoryPath(input.OutputDirectory, targetPlatformString);
    }

    private ImmutableArray<string> SystemIncludeDirectories(
        InputUnsanitized input,
        InputUnsanitizedTargetPlatform targetPlatformInput)
    {
        return SanitizeDirectoryPaths(
            input.SystemIncludeDirectories,
            targetPlatformInput.SystemIncludeDirectories);
    }

    private ImmutableArray<string> UserIncludeDirectories(
        InputUnsanitized input,
        InputUnsanitizedTargetPlatform targetPlatformInput,
        string inputFilePath)
    {
        return SanitizeUserIncludeDirectories(
            input.UserIncludeDirectories,
            targetPlatformInput.UserIncludeDirectories,
            inputFilePath);
    }

    private ImmutableArray<string> IgnoredIncludeFiles(
        InputUnsanitized input,
        InputUnsanitizedTargetPlatform targetPlatformInput)
    {
        return SanitizeDirectoryPaths(
            input.IgnoredIncludeFiles, targetPlatformInput.IgnoredIncludeDirectories);
    }

    private ImmutableDictionary<string, string> ClangDefines(
        InputUnsanitized input,
        InputUnsanitizedTargetPlatform targetPlatformInput)
    {
        return SanitizeStringsAndCombine(input.Defines, targetPlatformInput.Defines);
    }

    private ImmutableArray<string> ClangArguments(InputUnsanitizedTargetPlatform targetPlatformInput)
    {
        return SanitizeStrings(targetPlatformInput.ClangArguments);
    }

    private ImmutableArray<Regex> BlockedNameRegexes(InputUnsanitized input)
    {
        return SanitizeRegexes(input.BlockedNames);
    }

    private ImmutableArray<Regex> AllowedNameRegexes(InputUnsanitized input)
    {
        return SanitizeRegexes(input.AllowedNames);
    }

    private string SanitizeOutputDirectoryPath(
        string? outputDirectoryPath,
        string targetPlatformString)
    {
        string directoryPath;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (string.IsNullOrEmpty(outputDirectoryPath))
        {
            directoryPath = Path.Combine(Environment.CurrentDirectory, "ffi");
        }
        else
        {
            try
            {
                directoryPath = Path.GetFullPath(outputDirectoryPath);
            }
            catch (Exception e)
            {
                throw new InputSanitizationException($"Could not determine full directory path of specified output directory: {outputDirectoryPath}", e);
            }
        }

        var defaultFilePath = Path.Combine(directoryPath, targetPlatformString + ".json");
        return defaultFilePath;
    }

    private ImmutableArray<string> SanitizeUserIncludeDirectories(
        ImmutableArray<string>? unsanitizedUserIncludeDirectories1,
        ImmutableArray<string>? unsanitizedUserIncludeDirectories2,
        string inputFilePath)
    {
        var directoryPaths = SanitizeDirectoryPaths(
            unsanitizedUserIncludeDirectories1, unsanitizedUserIncludeDirectories2);

        if (directoryPaths.IsDefaultOrEmpty)
        {
            var directoryPath = Path.GetDirectoryName(inputFilePath)!;
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Environment.CurrentDirectory;
            }

            _ = directoryPaths.AddRange(Path.GetFullPath(directoryPath));
        }

        foreach (var directory in directoryPaths)
        {
            if (!Directory.Exists(directory))
            {
                throw new ToolInputSanitizationException($"The include directory does not exist: `{directory}`.");
            }
        }

        return directoryPaths;
    }
}
