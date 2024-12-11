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
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract;

[UsedImplicitly]
public sealed class ExtractFfiTool(
    ILogger<ExtractFfiTool> logger,
    IFileSystem fileSystem,
    ExtractInputSanitizer inputSanitizer,
    ClangInstaller clangInstaller,
    Explorer explorer) : Tool<UnsanitizedExtractInput, ExtractInput, ExtractOutput>(logger, inputSanitizer, fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    private string? _clangFilePath;

    public void Run(string configurationFilePath, string? clangFilePath = null)
    {
        _clangFilePath = clangFilePath;
        _ = base.Run(configurationFilePath);
    }

    protected override void Execute(ExtractInput input, ExtractOutput output)
    {
        BeginStep("Install libclang");
        var libClangIsInstalled = clangInstaller.TryInstall(_clangFilePath);
        EndStep();

        if (!libClangIsInstalled)
        {
            return;
        }

        foreach (var targetPlatformInput in input.TargetPlatformInputs)
        {
            BeginStep($"Extracting FFI {targetPlatformInput.TargetPlatform}");

            var ffi = explorer.ExtractFfi(
                input.InputFilePath,
                targetPlatformInput);
            Json.WriteFfiTargetPlatform(_fileSystem, targetPlatformInput.OutputFilePath, ffi);

            EndStep();
        }
    }
}
