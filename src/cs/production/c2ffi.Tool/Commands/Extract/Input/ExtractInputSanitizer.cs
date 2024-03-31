// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using c2ffi.Data;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;
using c2ffi.Tool.Commands.Extract.Input.Unsanitized;
using c2ffi.Tool.Internal.Input;

namespace c2ffi.Tool.Commands.Extract.Input;

public sealed class ExtractInputSanitizer : InputSanitizer<UnsanitizedExtractInput, ExtractOptions>
{
    private readonly string _hostOperatingSystemString;

    public ExtractInputSanitizer(IFileSystem fileSystem)
        : base(fileSystem)
    {
        _hostOperatingSystemString = Native.OperatingSystem.ToString().ToUpperInvariant();
    }

    protected override ExtractOptions Sanitize(UnsanitizedExtractInput unsanitizedInput)
    {
        unsanitizedInput.TargetPlatforms ??= CreateDefaultTargetPlatformsUnsanitizedInput();
        var inputFilePath = SanitizeCInputFilePath(unsanitizedInput.InputFilePath);
        var targetPlatformsOptions = SanitizeTargetPlatformsOptions(unsanitizedInput, inputFilePath);

        var result = new ExtractOptions
        {
            InputFilePath = inputFilePath,
            TargetPlatformsOptions = targetPlatformsOptions
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

    private ImmutableArray<ExtractTargetPlatformOptions> SanitizeTargetPlatformsOptions(
        UnsanitizedExtractInput unsanitizedInput,
        string inputFilePath)
    {
        var builder = ImmutableArray.CreateBuilder<ExtractTargetPlatformOptions>();

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

    private ExtractTargetPlatformOptions SanitizeTargetPlatformInput(
        UnsanitizedExtractInput input,
        string targetPlatformString,
        UnsanitizedExtractInputTargetPlatform targetPlatformInput,
        string inputFilePath)
    {
        var targetPlatform = new TargetPlatform(targetPlatformString);

        var outputFilePath = SanitizeOutputDirectoryPath(input.OutputDirectory, targetPlatformString);

        var systemIncludeDirectories =
            SanitizeDirectoryPaths(
                input.SystemIncludeDirectories,
                targetPlatformInput.SystemIncludeDirectories);

        var userIncludeDirectories = SanitizeUserIncludeDirectories(
            input.UserIncludeDirectories,
            targetPlatformInput.UserIncludeDirectories,
            inputFilePath);

        var ignoredIncludeFiles = SanitizeDirectoryPaths(
            input.IgnoredIncludeFiles, targetPlatformInput.IgnoredIncludeDirectories);

        var clangDefines = SanitizeStrings(targetPlatformInput.Defines);
        var clangArguments = SanitizeStrings(targetPlatformInput.ClangArguments);

        var opaqueTypeNames = SanitizeStrings(input.OpaqueTypeNames).ToImmutableHashSet();
        var allowedMacroObjects = SanitizeStrings(input.AllowedMacroObjects).ToImmutableHashSet();
        var allowedVariables = SanitizeStrings(input.AllowedVariables).ToImmutableHashSet();

        var options = new ExtractTargetPlatformOptions
        {
            TargetPlatform = targetPlatform,
            OutputFilePath = outputFilePath,
            UserIncludeDirectories = userIncludeDirectories,
            SystemIncludeDirectories = systemIncludeDirectories,
            IgnoredIncludeFiles = ignoredIncludeFiles,
            MacroObjectDefines = clangDefines,
            AdditionalArguments = clangArguments,
            IsEnabledFindSystemHeaders = input.IsEnabledAutomaticallyFindSystemHeaders ?? true,
            IsEnabledSystemDeclarations = input.IsEnabledSystemDeclarations ?? false,
            IsEnabledOnlyExternalTopLevelCursors = input.IsEnabledOnlyExternalTopLevelCursors ?? true,
            OpaqueTypeNames = opaqueTypeNames,
            AllowedMacroObjects = allowedMacroObjects,
            AllowedVariables = allowedVariables
        };

        return options;
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

            directoryPaths.AddRange(Path.GetFullPath(directoryPath));
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
