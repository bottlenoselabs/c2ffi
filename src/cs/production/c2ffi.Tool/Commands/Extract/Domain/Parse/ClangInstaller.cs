// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
using bottlenoselabs;
using c2ffi.Data;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract.Domain.Parse;

[UsedImplicitly]
public sealed partial class ClangInstaller
{
    private readonly ILogger<ClangInstaller> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IPath _path;

    private readonly Lock _lock = new();
    private string _clangNativeLibraryFilePath = null!;
    private bool _isInstalled;

    public ClangInstaller(
        ILogger<ClangInstaller> logger,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _path = _fileSystem.Path;
    }

    public bool TryInstall(string? clangFilePath = null)
    {
        using (_lock.EnterScope())
        {
            if (_isInstalled)
            {
                LogAlreadyInstalled(_clangNativeLibraryFilePath);
                return true;
            }

            var filePath = GetClangFilePath(clangFilePath);
            if (string.IsNullOrEmpty(filePath))
            {
                LogFailure();
                return false;
            }

            _clangNativeLibraryFilePath = filePath;
            NativeLibrary.SetDllImportResolver(typeof(clang).Assembly, ResolveClang);
            var clangVersion = clang.clang_getClangVersion().String();
            LogSuccessInstalled(filePath, clangVersion);
            _isInstalled = true;
            return true;
        }
    }

    private string GetClangFilePath(string? clangFilePath = null)
    {
        if (!string.IsNullOrEmpty(clangFilePath))
        {
            return clangFilePath;
        }

#pragma warning disable IDE0072
        var result = Native.OperatingSystem switch
#pragma warning restore IDE0072
        {
            NativeOperatingSystem.Windows => GetClangFilePathWindows(),
            NativeOperatingSystem.Linux => GetClangFilePathLinux(),
            NativeOperatingSystem.macOS => GetClangFilePathMacOs(),
            _ => throw new NotImplementedException()
        };

        return result;
    }

    private string GetClangFilePathWindows()
    {
        var filePaths = new[]
        {
            // ReSharper disable StringLiteralTypo
            _path.Combine(Environment.CurrentDirectory, "libclang.dll"),
            _path.Combine(Environment.CurrentDirectory, "clang.dll"),
            _path.Combine(AppContext.BaseDirectory, "libclang.dll"),
            _path.Combine(AppContext.BaseDirectory, "clang.dll"),
            @"C:\Program Files\LLVM\bin\libclang.dll" // choco install llvm
            // ReSharper restore StringLiteralTypo
        };

        var result = SearchForClangFilePath(filePaths);
        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }

        var errorMessage =
            "`libclang.dll` or `clang.dll` is missing. Tried searching the following" +
            $" paths: \"{string.Join(", ", filePaths)}\"." +
            " Please put a `libclang.dll` or `clang.dll` file next to this application or install Clang for Windows." +
            " To install Clang for Windows using Chocolatey, use the command `choco install llvm`.";
        throw new InvalidOperationException(errorMessage);
    }

    private string GetClangFilePathLinux()
    {
        var filePaths = new[]
        {
            // ReSharper disable StringLiteralTypo
            _path.Combine(Environment.CurrentDirectory, "libclang.so"),
            _path.Combine(AppContext.BaseDirectory, "libclang.so"),
            "/usr/lib/libclang.so",

            // found via running the following command on Ubuntu 20.04: find / -name libclang.so* 2>/dev/null
            "/usr/lib/llvm-14/lib/libclang.so.1",
            "/usr/lib/llvm-13/lib/libclang.so.1",
            "/usr/lib/llvm-12/lib/libclang.so.1",

            // legacy fills
            "/usr/lib/llvm-11/lib/libclang.so.1",
            "/usr/lib/llvm-10/lib/libclang.so.1"
            // ReSharper restore StringLiteralTypo
        };

        var result = SearchForClangFilePath(filePaths);
        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }

        var errorMessage =
            $"`libclang.so` is missing. Tried searching the following paths: \"{string.Join(", ", filePaths)}\"." +
            " Please put a `libclang.so` file next to this application or install Clang for Linux. To install Clang" +
            " for Debian-based Linux distributions, use the command `apt-get update && apt-get install clang`.";
        throw new InvalidOperationException(errorMessage);
    }

    private string GetClangFilePathMacOs()
    {
        var filePaths = new[]
        {
            // ReSharper disable StringLiteralTypo
            _path.Combine(Environment.CurrentDirectory, "libclang.dylib"),
            _path.Combine(AppContext.BaseDirectory, "libclang.dylib"),
            "/Library/Developer/CommandLineTools/usr/lib/libclang.dylib", // xcode-select --install
            "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/libclang.dylib", // XCode
            // ReSharper restore StringLiteralTypo
        };

        var result = SearchForClangFilePath(filePaths);
        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }

        var errorMessage =
            $"`libclang.dylib` is missing. Tried searching the following paths: \"{string.Join(", ", filePaths)}\"." +
            " Please put a `libclang.dylib` file next to this application or install CommandLineTools for macOS using" +
            " the command `xcode-select --install`.";
        throw new InvalidOperationException(errorMessage);
    }

    private string SearchForClangFilePath(params string[] filePaths)
    {
        var installedFilePath = string.Empty;
        foreach (var filePath in filePaths)
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                continue;
            }

            installedFilePath = filePath;
            break;
        }

        return installedFilePath;
    }

    private IntPtr ResolveClang(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!NativeLibrary.TryLoad(_clangNativeLibraryFilePath, out var handle))
        {
            throw new InvalidOperationException($"Could not load libclang: {_clangNativeLibraryFilePath}");
        }

        return handle;
    }

    [LoggerMessage(0, LogLevel.Error, "- Failure, could not determine path to libclang")]
    private partial void LogFailure();

    [LoggerMessage(1, LogLevel.Information, "- Success, installed, file path: {FilePath}, version: {Version}")]
    private partial void LogSuccessInstalled(string filePath, string version);

    [LoggerMessage(2, LogLevel.Information, "- Success, already installed, file path: {FilePath}")]
    private partial void LogAlreadyInstalled(string filePath);
}
