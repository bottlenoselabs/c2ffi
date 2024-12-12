// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace c2ffi.Data;

/// <summary>
///     Provides native utility functions.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static class Native
{
    /// <summary>
    ///     Gets the <see cref="NativeOperatingSystem" /> of the current application.
    /// </summary>
    public static NativeOperatingSystem OperatingSystem
    {
        get
        {
            if (System.OperatingSystem.IsWindows())
            {
                return NativeOperatingSystem.Windows;
            }

            if (System.OperatingSystem.IsMacOS())
            {
                return NativeOperatingSystem.macOS;
            }

            if (System.OperatingSystem.IsLinux())
            {
                return NativeOperatingSystem.Linux;
            }

            if (System.OperatingSystem.IsAndroid())
            {
                return NativeOperatingSystem.Android;
            }

            if (System.OperatingSystem.IsIOS())
            {
                return NativeOperatingSystem.iOS;
            }

            if (System.OperatingSystem.IsTvOS())
            {
                return NativeOperatingSystem.tvOS;
            }

            if (System.OperatingSystem.IsBrowser())
            {
                return NativeOperatingSystem.Browser;
            }

            return NativeOperatingSystem.Unknown;
        }
    }

    /// <summary>
    ///     Gets the <see cref="NativeArchitecture" /> of the current application.
    /// </summary>
    public static NativeArchitecture Architecture => RuntimeInformation.OSArchitecture switch
    {
        System.Runtime.InteropServices.Architecture.Arm64 => NativeArchitecture.ARM64,
        System.Runtime.InteropServices.Architecture.Arm => NativeArchitecture.ARM32,
        System.Runtime.InteropServices.Architecture.X86 => NativeArchitecture.X86,
        System.Runtime.InteropServices.Architecture.X64 => NativeArchitecture.X64,
        System.Runtime.InteropServices.Architecture.Wasm => NativeArchitecture.WASM32,
        System.Runtime.InteropServices.Architecture.S390x => NativeArchitecture.Unknown,
        System.Runtime.InteropServices.Architecture.LoongArch64 => NativeArchitecture.Unknown,
        System.Runtime.InteropServices.Architecture.Armv6 => NativeArchitecture.Unknown,
        System.Runtime.InteropServices.Architecture.Ppc64le => NativeArchitecture.Unknown,
        System.Runtime.InteropServices.Architecture.RiscV64 => NativeArchitecture.Unknown,
        _ => NativeArchitecture.Unknown
    };

    /// <summary>
    ///     Gets the <see cref="TargetPlatform" /> of the current application.
    /// </summary>
    public static TargetPlatform Platform
    {
        get
        {
            var operatingSystem = OperatingSystem;
            var architecture = Architecture;

            return operatingSystem switch
            {
                NativeOperatingSystem.Windows when architecture == NativeArchitecture.X64 => TargetPlatform
                    .x86_64_pc_windows_gnu,
                NativeOperatingSystem.Windows when architecture == NativeArchitecture.X86 => TargetPlatform
                    .i686_pc_windows_gnu,
                NativeOperatingSystem.Windows when architecture == NativeArchitecture.ARM64 => TargetPlatform
                    .aarch64_pc_windows_gnu,
                NativeOperatingSystem.macOS when architecture == NativeArchitecture.ARM64 => TargetPlatform
                    .aarch64_apple_darwin,
                NativeOperatingSystem.macOS when architecture == NativeArchitecture.X64 => TargetPlatform
                    .x86_64_apple_darwin,
                NativeOperatingSystem.Linux when architecture == NativeArchitecture.X64 => TargetPlatform
                    .x86_64_unknown_linux_gnu,
                NativeOperatingSystem.Linux when architecture == NativeArchitecture.X86 => TargetPlatform
                    .i686_unknown_linux_gnu,
                NativeOperatingSystem.Linux when architecture == NativeArchitecture.ARM64 => TargetPlatform
                    .aarch64_unknown_linux_gnu,
                NativeOperatingSystem.Unknown => TargetPlatform.Unknown,
                NativeOperatingSystem.FreeBSD => TargetPlatform.Unknown,
                NativeOperatingSystem.Android => TargetPlatform.Unknown,
                NativeOperatingSystem.iOS => TargetPlatform.Unknown,
                NativeOperatingSystem.tvOS => TargetPlatform.Unknown,
                NativeOperatingSystem.Browser => TargetPlatform.Unknown,
                NativeOperatingSystem.PlayStation4 => TargetPlatform.Unknown,
                NativeOperatingSystem.Xbox => TargetPlatform.Unknown,
                NativeOperatingSystem.Switch => TargetPlatform.Unknown,
                NativeOperatingSystem.DualScreen3D => TargetPlatform.Unknown,
                _ => TargetPlatform.Unknown
            };
        }
    }
}
