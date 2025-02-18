// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Native = c2ffi.Data.Native;
using NativeArchitecture = c2ffi.Data.NativeArchitecture;
using NativeOperatingSystem = c2ffi.Data.NativeOperatingSystem;
using TargetPlatform = c2ffi.Data.TargetPlatform;

namespace c2ffi.Extract.Parse;

[UsedImplicitly]
public sealed partial class ParseSystemIncludeDirectoriesProvider(
    ILogger<ParseSystemIncludeDirectoriesProvider> logger,
    IFileSystem fileSystem)
{
    public ImmutableArray<string> GetSystemIncludeDirectories(
        TargetPlatform targetPlatform,
        ImmutableArray<string> userProvidedSystemIncludeDirectories,
        bool isEnabledFindSystemHeaders = true)
    {
        var systemIncludeDirectories =
            isEnabledFindSystemHeaders ? FindSystemIncludeDirectories(targetPlatform, userProvidedSystemIncludeDirectories) : userProvidedSystemIncludeDirectories;

        var builder = ImmutableArray.CreateBuilder<string>();
        foreach (var directory in systemIncludeDirectories)
        {
            if (fileSystem.Directory.Exists(directory))
            {
                builder.Add(directory);
            }
            else
            {
                LogMissingSystemIncludeDirectory(directory);
            }
        }

        return builder.ToImmutable();
    }

    private ImmutableArray<string> FindSystemIncludeDirectories(
        TargetPlatform targetPlatform,
        ImmutableArray<string> userProvidedSystemIncludeDirectories)
    {
        var hostOperatingSystem = Native.OperatingSystem;
        var hostArchitecture = Native.Architecture;
        var directories = ImmutableArray.CreateBuilder<string>();

#pragma warning disable IDE0010
        switch (hostOperatingSystem)
#pragma warning restore IDE0010
        {
            case NativeOperatingSystem.Windows:
                {
                    FindSystemIncludeDirectoriesHostWindows(targetPlatform, directories);
                    break;
                }

            case NativeOperatingSystem.macOS:
                {
                    FindSystemIncludeDirectoriesHostMac(targetPlatform, directories);
                    break;
                }

            case NativeOperatingSystem.Linux:
                {
                    FindSystemIncludeDirectoriesHostLinux(targetPlatform, hostArchitecture, directories);
                    break;
                }

            default:
                throw new NotImplementedException();
        }

        directories.AddRange(userProvidedSystemIncludeDirectories);

        var result = directories.Distinct().ToImmutableArray();
        return result;
    }

    private void FindSystemIncludeDirectoriesHostWindows(
        TargetPlatform targetPlatform,
        ImmutableArray<string>.Builder directories)
    {
#pragma warning disable IDE0010
        switch (targetPlatform.OperatingSystem)
#pragma warning restore IDE0010
        {
            case NativeOperatingSystem.Windows:
                FindSystemIncludeDirectoriesTargetWindows(directories);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetPlatform));
        }
    }

    private void FindSystemIncludeDirectoriesHostMac(
        TargetPlatform targetPlatform,
        ImmutableArray<string>.Builder directories)
    {
        var clangVersionDirectory = GetHighestVersionDirectoryPathFrom("/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang");
        if (!string.IsNullOrEmpty(clangVersionDirectory))
        {
            directories.Add($"{clangVersionDirectory}/include");
        }

#pragma warning disable IDE0010
        switch (targetPlatform.OperatingSystem)
#pragma warning restore IDE0010
        {
            case NativeOperatingSystem.macOS:
                FindSystemIncludesDirectoriesTargetMac(directories);
                break;
            case NativeOperatingSystem.iOS:
                FindSystemIncludesDirectoriesTargetIPhone(directories);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetPlatform));
        }
    }

    private void FindSystemIncludeDirectoriesHostLinux(
        TargetPlatform targetPlatform,
        NativeArchitecture hostArchitecture,
        ImmutableArray<string>.Builder directories)
    {
#pragma warning disable IDE0010
        switch (targetPlatform.OperatingSystem)
#pragma warning restore IDE0010
        {
            case NativeOperatingSystem.Linux:
                FindSystemIncludeDirectoriesTargetLinux(hostArchitecture, targetPlatform.Architecture, directories);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetPlatform));
        }
    }

    private void FindSystemIncludeDirectoriesTargetWindows(ImmutableArray<string>.Builder directories)
    {
        var sdkDirectoryPath =
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Windows Kits\10\Include");
        if (!string.IsNullOrEmpty(sdkDirectoryPath) && !fileSystem.Directory.Exists(sdkDirectoryPath))
        {
            throw new ClangException(
                "Please install the software development kit (SDK) for Windows 10: https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/");
        }

        var sdkHighestVersionDirectoryPath = GetHighestVersionDirectoryPathFrom(sdkDirectoryPath);
        if (string.IsNullOrEmpty(sdkHighestVersionDirectoryPath))
        {
            throw new ClangException(
                $"Unable to find a Windows SDK version. Expected a Windows SDK version at '{sdkDirectoryPath}'."
            + " Do you need install the software development kit for Windows? https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk/");
        }

        // e.g. ucrt, shared, etc
        var sdkDirectoryPaths = fileSystem.Directory.EnumerateDirectories(sdkHighestVersionDirectoryPath);
        foreach (var directoryPath in sdkDirectoryPaths)
        {
            directories.Add(directoryPath);
        }

        var vsWhereFilePath =
            Environment.ExpandEnvironmentVariables(
                @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe");
        var shellOutput = "-latest -property installationPath".ExecuteShellCommand(fileName: vsWhereFilePath);
        var visualStudioInstallationDirectoryPath = shellOutput.Output.Trim();
        if (!fileSystem.File.Exists(vsWhereFilePath) || string.IsNullOrEmpty(visualStudioInstallationDirectoryPath))
        {
            throw new ClangException(
                "Please install Visual Studio 2017 or later (community, professional, or enterprise).");
        }

        var mscvVersionsDirectoryPath = fileSystem.Path.Combine(visualStudioInstallationDirectoryPath, @"VC\Tools\MSVC");
        if (!fileSystem.Directory.Exists(mscvVersionsDirectoryPath))
        {
            throw new ClangException(
                $"Please install the Microsoft Visual C++ (MSVC) build tools for Visual Studio ({visualStudioInstallationDirectoryPath}).");
        }

        var mscvHighestVersionDirectoryPath = GetHighestVersionDirectoryPathFrom(mscvVersionsDirectoryPath);
        if (string.IsNullOrEmpty(mscvHighestVersionDirectoryPath))
        {
            throw new ClangException(
                $"Unable to find a version for Microsoft Visual C++ (MSVC) build tools for Visual Studio ({visualStudioInstallationDirectoryPath}).");
        }

        var mscvIncludeDirectoryPath = fileSystem.Path.Combine(mscvHighestVersionDirectoryPath, "include");
        if (!fileSystem.Directory.Exists(mscvIncludeDirectoryPath))
        {
            throw new ClangException(
                $"Please install Microsoft Visual C++ (MSVC) build tools for Visual Studio ({visualStudioInstallationDirectoryPath}).");
        }

        directories.Add(mscvIncludeDirectoryPath);
    }

    private void FindSystemIncludesDirectoriesTargetMac(ImmutableArray<string>.Builder directories)
    {
        var shellOutput = "xcrun --sdk macosx --show-sdk-path".ExecuteShellCommand();
        var sdkPath = shellOutput.Output.Trim();
        if (!fileSystem.Directory.Exists(sdkPath))
        {
            throw new ClangException(
                "Please install XCode or CommandLineTools for macOS."
                + " This will install the software development kit (SDK) for macOS which gives access to common C/C++/ObjC headers.");
        }

        directories.Add($"{sdkPath}/usr/include");
    }

    private void FindSystemIncludesDirectoriesTargetIPhone(ImmutableArray<string>.Builder directories)
    {
        var shellOutput = "xcrun --sdk iphoneos --show-sdk-path".ExecuteShellCommand();
        var sdkPath = shellOutput.Output.Trim();
        if (!fileSystem.Directory.Exists(sdkPath))
        {
            throw new ClangException(
                "Please install XCode for macOS." +
                " This will install the software development kit (SDK) for iOS which gives access to common C/C++/ObjC headers.");
        }

        directories.Add($"{sdkPath}/usr/include");
    }

    private void FindSystemIncludeDirectoriesTargetLinux(
        NativeArchitecture hostArchitecture,
        NativeArchitecture targetArchitecture,
        ImmutableArray<string>.Builder directories)
    {
        directories.Add("/usr/include");

        var clangVersionDirectory = GetHighestVersionDirectoryPathFrom("/usr/include/clang");
        if (!string.IsNullOrEmpty(clangVersionDirectory))
        {
            directories.Add($"{clangVersionDirectory}/include");
        }

        // Cross platform headers are in: /usr/[ARCH]-linux-gnu/include
        //  For Ubuntu, cross platform toolchain (includes headers) are installed via packages:
        //  - gcc-x86-64-linux-gnu (ARCH = x86_64)
        //  - gcc-aarch64-linux-gnu (ARCH = aarch64)
        //  - gcc-i686-linux-gnu (ARCH = i686)
        // Host headers are in /usr/include/[ARCH]-linux-gnu

        if (targetArchitecture == hostArchitecture)
        {
            if (targetArchitecture == NativeArchitecture.X64)
            {
                directories.Add("/usr/include/x86_64-linux-gnu");
            }
            else if (targetArchitecture == NativeArchitecture.ARM64)
            {
                directories.Add("/usr/include/aarch64-linux-gnu");
            }
            else if (targetArchitecture == NativeArchitecture.X86)
            {
                directories.Add("/usr/include/i686-linux-gnu");
            }
        }
        else
        {
            if (targetArchitecture == NativeArchitecture.X64)
            {
                directories.Add("/usr/x86_64-linux-gnu/include");
            }
            else if (targetArchitecture == NativeArchitecture.ARM64)
            {
                directories.Add("/usr/aarch64-linux-gnu/include");
            }
            else if (targetArchitecture == NativeArchitecture.X86)
            {
                directories.Add("/usr/i686-linux-gnu/include");
            }
        }
    }

    private string GetHighestVersionDirectoryPathFrom(string directoryPath)
    {
        var versionDirectoryPaths = fileSystem.Directory.EnumerateDirectories(directoryPath);
        var result = string.Empty;
        var highestVersion = Version.Parse("0.0.0");

        foreach (var versionDirectoryPath in versionDirectoryPaths)
        {
            var versionStringIndex = versionDirectoryPath.LastIndexOf(fileSystem.Path.DirectorySeparatorChar);
            var versionString = versionDirectoryPath[(versionStringIndex + 1)..];
            if (!Version.TryParse(versionString, out var version))
            {
                continue;
            }

            if (version < highestVersion)
            {
                continue;
            }

            highestVersion = version;
            result = versionDirectoryPath;
        }

        return result;
    }

    [LoggerMessage(0, LogLevel.Warning, "- Could not find system include directory: {DirectoryPath}")]
    private partial void LogMissingSystemIncludeDirectory(string directoryPath);
}
