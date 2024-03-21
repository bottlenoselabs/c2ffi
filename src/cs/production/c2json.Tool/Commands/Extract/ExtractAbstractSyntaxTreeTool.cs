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

public sealed partial class ExtractAbstractSyntaxTreeTool
{
    private readonly ILogger<ExtractAbstractSyntaxTreeTool> _logger;

    private readonly IFileSystem _fileSystem;
    private readonly ExtractInputSanitizer _inputSanitizer;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;

    public ExtractAbstractSyntaxTreeTool(
        ILogger<ExtractAbstractSyntaxTreeTool> logger,
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
        var targetPlatforms = ExtractAbstractSyntaxTrees(options);
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

    private ImmutableArray<TargetPlatform> ExtractAbstractSyntaxTrees(ExtractOptions options)
    {
        var builder = ImmutableArray.CreateBuilder<TargetPlatform>();

        foreach (var astOptions in options.TargetPlatformsOptions)
        {
            var targetPlatform = ExtractAbstractSyntaxTree(options, astOptions);
            if (targetPlatform != null)
            {
                builder.Add(targetPlatform.Value);
            }
        }

        return builder.ToImmutable();
    }

    private TargetPlatform? ExtractAbstractSyntaxTree(
        ExtractOptions options,
        ExtractTargetPlatformOptions targetPlatformOptions)
    {
        try
        {
            var abstractSyntaxTree = _explorer.GetAbstractSyntaxTree(
                options.InputFilePath,
                targetPlatformOptions);
            Json.WriteAbstractSyntaxTreeTargetPlatform(_fileSystem, targetPlatformOptions.OutputFilePath, abstractSyntaxTree);
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            LogWriteAbstractSyntaxTreeTargetPlatformFailure(e, targetPlatformOptions.TargetPlatform, targetPlatformOptions.OutputFilePath);
            return null;
        }

        LogWriteAbstractSyntaxTreeTargetPlatformSuccess(targetPlatformOptions.TargetPlatform, targetPlatformOptions.OutputFilePath);
        return targetPlatformOptions.TargetPlatform;
    }

    [LoggerMessage(0, LogLevel.Information, "Success. Extracted abstract syntax tree for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LogWriteAbstractSyntaxTreeTargetPlatformSuccess(
        TargetPlatform targetPlatform,
        string filePath);

    [LoggerMessage(1, LogLevel.Error, "Failed to extract abstract syntax tree for the target platform '{TargetPlatform}': {FilePath}")]
    private partial void LogWriteAbstractSyntaxTreeTargetPlatformFailure(
        Exception exception,
        TargetPlatform targetPlatform,
        string filePath);

    [LoggerMessage(2, LogLevel.Information, "Success. Extracted abstract syntax trees for the target platforms '{TargetPlatforms}'.")]
    private partial void LogSuccess(
        ImmutableArray<TargetPlatform> targetPlatforms);

    [LoggerMessage(3, LogLevel.Error, "Failure.")]
    private partial void LogFailure();
}
