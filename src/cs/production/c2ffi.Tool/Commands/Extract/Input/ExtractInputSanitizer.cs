// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using bottlenoselabs.Common.Tools;
using c2ffi.Data;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;
using c2ffi.Tool.Commands.Extract.Input.Unsanitized;
using c2ffi.Tool.Internal.Input;
using JetBrains.Annotations;

namespace c2ffi.Tool.Commands.Extract.Input;

[UsedImplicitly]
public sealed class ExtractInputSanitizer(IFileSystem fileSystem) : InputSanitizer<UnsanitizedExtractInput, ExtractInput>(fileSystem)
{
    private readonly string _hostOperatingSystemString = Native.OperatingSystem.ToString().ToUpperInvariant();

    public override ExtractInput Sanitize(UnsanitizedExtractInput unsanitizedInput)
    {
        unsanitizedInput.TargetPlatforms ??= CreateDefaultTargetPlatformsUnsanitizedInput();
        var inputFilePath = SanitizeCInputFilePath(unsanitizedInput.InputFilePath);
        var targetPlatformsOptions = SanitizeTargetPlatformsOptions(unsanitizedInput, inputFilePath);

        var result = new ExtractInput
        {
            InputFilePath = inputFilePath,
            TargetPlatformInputs = targetPlatformsOptions
        };

        return result;
    }

    private ImmutableDictionary<string, ImmutableDictionary<string, UnsanitizedExtractInputTargetPlatform>> CreateDefaultTargetPlatformsUnsanitizedInput()
    {
        var extractOptionsByTargetPlatformString = new Dictionary<string, UnsanitizedExtractInputTargetPlatform>();
        var targetPlatformString = Native.Platform.ToString();
        extractOptionsByTargetPlatformString.Add(targetPlatformString, new UnsanitizedExtractInputTargetPlatform());

        var targetPlatformStringsByOperatingSystemString = new Dictionary<string, ImmutableDictionary<string, UnsanitizedExtractInputTargetPlatform>>
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

    private ImmutableArray<ExtractTargetPlatformInput> SanitizeTargetPlatformsOptions(
        UnsanitizedExtractInput unsanitizedInput,
        string inputFilePath)
    {
        var builder = ImmutableArray.CreateBuilder<ExtractTargetPlatformInput>();

        var isAtLeastOneMatchingOperatingSystem = false;
        var targetPlatformsUnsanitizedInputByOperatingSystem = unsanitizedInput.TargetPlatforms;
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
                    var targetPlatformOptions = SanitizeTargetPlatformInput(unsanitizedInput, targetPlatformString, unsanitizedTargetPlatformInput, inputFilePath);
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

    private ExtractTargetPlatformInput SanitizeTargetPlatformInput(
        UnsanitizedExtractInput input,
        string targetPlatformString,
        UnsanitizedExtractInputTargetPlatform targetPlatformInput,
        string inputFilePath)
    {
        var targetPlatform = new TargetPlatform(targetPlatformString);
        var options = new ExtractTargetPlatformInput
        {
            TargetPlatform = targetPlatform,
            OutputFilePath = OutputFilePath(input, targetPlatformString),
            SystemIncludeDirectories = SystemIncludeDirectories(input, targetPlatformInput),
            UserIncludeDirectories = UserIncludeDirectories(input, targetPlatformInput, inputFilePath),
            IgnoredIncludeFiles = IgnoredIncludeFiles(input, targetPlatformInput),
            MacroObjectDefines = ClangDefines(input, targetPlatformInput),
            AdditionalArguments = ClangArguments(targetPlatformInput),
            IsEnabledFindSystemHeaders = input.IsEnabledAutomaticallyFindSystemHeaders ?? true,
            IsSingleHeader = input.IsSingleHeader ?? false,
            IncludedNames = IncludedNames(input),
            IgnoredMacroObjectsRegexes = IgnoredMacroObjects(input),
            IgnoredVariableRegexes = IgnoredVariables(input),
            IgnoredFunctionRegexes = IgnoredFunctions(input)
        };

        return options;
    }

    private string OutputFilePath(UnsanitizedExtractInput input, string targetPlatformString)
    {
        return SanitizeOutputDirectoryPath(input.OutputDirectory, targetPlatformString);
    }

    private ImmutableArray<string> SystemIncludeDirectories(
        UnsanitizedExtractInput input,
        UnsanitizedExtractInputTargetPlatform targetPlatformInput)
    {
        return SanitizeDirectoryPaths(
            input.SystemIncludeDirectories,
            targetPlatformInput.SystemIncludeDirectories);
    }

    private ImmutableArray<string> UserIncludeDirectories(
        UnsanitizedExtractInput input,
        UnsanitizedExtractInputTargetPlatform targetPlatformInput,
        string inputFilePath)
    {
        return SanitizeUserIncludeDirectories(
            input.UserIncludeDirectories,
            targetPlatformInput.UserIncludeDirectories,
            inputFilePath);
    }

    private ImmutableArray<string> IgnoredIncludeFiles(
        UnsanitizedExtractInput input,
        UnsanitizedExtractInputTargetPlatform targetPlatformInput)
    {
        return SanitizeDirectoryPaths(
            input.IgnoredIncludeFiles, targetPlatformInput.IgnoredIncludeDirectories);
    }

    private ImmutableDictionary<string, string> ClangDefines(
        UnsanitizedExtractInput input,
        UnsanitizedExtractInputTargetPlatform targetPlatformInput)
    {
        return SanitizeStringsAndCombine(input.Defines, targetPlatformInput.Defines);
    }

    private ImmutableArray<string> ClangArguments(UnsanitizedExtractInputTargetPlatform targetPlatformInput)
    {
        return SanitizeStrings(targetPlatformInput.ClangArguments);
    }

    private ImmutableArray<Regex> IgnoredMacroObjects(UnsanitizedExtractInput input)
    {
        return SanitizeRegexes(input.IgnoredMacroObjects);
    }

    private ImmutableArray<Regex> IgnoredVariables(UnsanitizedExtractInput input)
    {
        return SanitizeRegexes(input.IgnoredVariables);
    }

    private ImmutableArray<Regex> IgnoredFunctions(UnsanitizedExtractInput input)
    {
        return SanitizeRegexes(input.IgnoredFunctions);
    }

    private ImmutableHashSet<string> IncludedNames(UnsanitizedExtractInput input)
    {
        return [.. SanitizeStrings(input.IncludedNames)];
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
