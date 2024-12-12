// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using c2ffi.Data.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ClangInstaller = c2ffi.Extract.Parse.ClangInstaller;
using Explorer = c2ffi.Extract.Explore.Explorer;

namespace c2ffi.Extract;

[UsedImplicitly]
public sealed class Tool(
    ILogger<Tool> logger,
    IFileSystem fileSystem,
    InputSanitizer inputSanitizer,
    ClangInstaller clangInstaller,
    Explorer explorer) : Tool<InputUnsanitized, InputSanitized, Output>(logger, inputSanitizer, fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    private string? _clangFilePath;

    public void Run(string configurationFilePath, string? clangFilePath = null)
    {
        _clangFilePath = clangFilePath;
        _ = base.Run(configurationFilePath);
    }

    protected override void Execute(InputSanitized inputSanitized, Output output)
    {
        BeginStep("Install libclang");
        var libClangIsInstalled = clangInstaller.TryInstall(_clangFilePath);
        EndStep();

        if (!libClangIsInstalled)
        {
            return;
        }

        foreach (var targetPlatformInput in inputSanitized.TargetPlatformInputs)
        {
            BeginStep($"Extracting FFI {targetPlatformInput.TargetPlatform}");

            var ffi = explorer.ExtractFfi(
                inputSanitized.InputFilePath,
                targetPlatformInput);
            Json.WriteFfiTargetPlatform(_fileSystem, targetPlatformInput.OutputFilePath, ffi);

            EndStep();
        }
    }
}
