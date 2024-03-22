// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using c2json.Data;
using c2json.Data.Serialization;
using c2json.Tool.Commands.Extract.Domain.Explore;
using c2json.Tool.Commands.Extract.Input;
using c2json.Tool.Commands.Extract.Input.Sanitized;
using Microsoft.Extensions.Logging;
using ClangInstaller = c2json.Tool.Commands.Extract.Domain.Parse.ClangInstaller;
using ExtractOptions = c2json.Tool.Commands.Extract.Input.ExtractOptions;

namespace c2json.Tool.Commands.Extract;

public sealed partial class ExtractFfiTool
{
    private readonly ILogger<ExtractFfiTool> _logger;

    private readonly IFileSystem _fileSystem;
    private readonly ExtractInputSanitizer _inputSanitizer;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;

    public ExtractFfiTool(
        ILogger<ExtractFfiTool> logger,
        IFileSystem fileSystem,
        ExtractInputSanitizer inputSanitizer,
        ClangInstaller clangInstaller,
        Explorer explorer)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _inputSanitizer = inputSanitizer;
        _clangInstaller = clangInstaller;
        _explorer = explorer;
    }

    public void Run(string configurationFilePath, string? clangFilePath = null)
    {
        if (!TryInstallClang(clangFilePath))
        {
            return;
        }

        var options = GetOptions(configurationFilePath);
        var targetPlatforms = ExtractFfis(options);
        if (targetPlatforms.IsDefaultOrEmpty)
        {
            LogFailure();
        }
        else
        {
            LogSuccess(targetPlatforms);
        }
    }

    private bool TryInstallClang(string? clangFilePath = null)
    {
        return _clangInstaller.TryInstall(clangFilePath);
    }

    private ExtractOptions GetOptions(string configurationFilePath)
    {
        return _inputSanitizer.SanitizeFromFile(configurationFilePath);
    }

    private ImmutableArray<TargetPlatform> ExtractFfis(ExtractOptions options)
    {
        var builder = ImmutableArray.CreateBuilder<TargetPlatform>();

        foreach (var targetPlatformOptions in options.TargetPlatformsOptions)
        {
            var targetPlatform = ExtractFfi(options, targetPlatformOptions);
            if (targetPlatform != null)
            {
                builder.Add(targetPlatform.Value);
            }
        }

        return builder.ToImmutable();
    }

    private TargetPlatform? ExtractFfi(
        ExtractOptions options,
        ExtractTargetPlatformOptions targetPlatformOptions)
    {
        try
        {
            var ffi = _explorer.ExtractFfi(
                options.InputFilePath,
                targetPlatformOptions);
            Json.WriteFfiTargetPlatform(_fileSystem, targetPlatformOptions.OutputFilePath, ffi);
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            LogWriteFfiTargetPlatformFailure(e, targetPlatformOptions.TargetPlatform, targetPlatformOptions.OutputFilePath);
            return null;
        }

        LogWriteFfiTargetPlatformSuccess(targetPlatformOptions.TargetPlatform, targetPlatformOptions.OutputFilePath);
        return targetPlatformOptions.TargetPlatform;
    }

    [LoggerMessage(0, LogLevel.Information, "Success. Extracted FFI for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LogWriteFfiTargetPlatformSuccess(
        TargetPlatform targetPlatform,
        string filePath);

    [LoggerMessage(1, LogLevel.Error, "Failed to extract FFI for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LogWriteFfiTargetPlatformFailure(
        Exception exception,
        TargetPlatform targetPlatform,
        string filePath);

    [LoggerMessage(2, LogLevel.Information, "Success. Extracted FFIs for the target platforms '{TargetPlatforms}'.")]
    private partial void LogSuccess(
        ImmutableArray<TargetPlatform> targetPlatforms);

    [LoggerMessage(3, LogLevel.Error, "Failure.")]
    private partial void LogFailure();
}
