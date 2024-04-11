// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using c2ffi.Data.Serialization;
using c2ffi.Tool.Commands.Extract.Domain.Explore;
using c2ffi.Tool.Commands.Extract.Domain.Parse;
using c2ffi.Tool.Commands.Extract.Input;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;
using c2ffi.Tool.Commands.Extract.Input.Unsanitized;
using c2ffi.Tool.Commands.Extract.Output;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract;

public sealed class ExtractFfiTool : Tool<UnsanitizedExtractInput, ExtractInput, ExtractOutput>
{
    private readonly IFileSystem _fileSystem;
    private readonly ClangInstaller _clangInstaller;
    private readonly Explorer _explorer;

    private string? _clangFilePath;

    public ExtractFfiTool(
        ILogger<ExtractFfiTool> logger,
        IFileSystem fileSystem,
        ExtractInputSanitizer inputSanitizer,
        ClangInstaller clangInstaller,
        Explorer explorer)
        : base(logger, inputSanitizer, fileSystem)
    {
        _fileSystem = fileSystem;
        _clangInstaller = clangInstaller;
        _explorer = explorer;
    }

    public void Run(string configurationFilePath, string? clangFilePath = null)
    {
        _clangFilePath = clangFilePath;
        base.Run(configurationFilePath);
    }

    protected override void Execute(ExtractInput input, ExtractOutput output)
    {
        BeginStep("Install libclang");
        var libClangIsInstalled = _clangInstaller.TryInstall(_clangFilePath);
        EndStep();

        if (!libClangIsInstalled)
        {
            return;
        }

        foreach (var targetPlatformInput in input.TargetPlatformInputs)
        {
            BeginStep($"Extracting FFI {targetPlatformInput.TargetPlatform}");

            var ffi = _explorer.ExtractFfi(
                input.InputFilePath,
                targetPlatformInput);
            Json.WriteFfiTargetPlatform(_fileSystem, targetPlatformInput.OutputFilePath, ffi);

            EndStep();
        }
    }
}
