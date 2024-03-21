// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2json.Data;
using c2json.Tool.Commands.Extract.Input.Sanitized;
using TargetPlatform = c2json.Data.TargetPlatform;

namespace c2json.Tool.Commands.Extract.Domain.Parse;

public class ParseContextBuilderArgumentsProvider
{
    public ImmutableArray<string> GetArguments(
        ExtractTargetPlatformOptions options,
        ImmutableArray<string> systemIncludeDirectories,
        bool isCPlusPlus,
        bool ignoreWarnings)
    {
        var args = ImmutableArray.CreateBuilder<string>();

        AddDefaults(args, options.TargetPlatform, isCPlusPlus, ignoreWarnings);
        AddUserIncludeDirectories(args, options.UserIncludeDirectories);
        AddDefines(args, options.MacroObjectDefines);
        AddTargetTriple(args, options.TargetPlatform);
        AddAdditionalArgs(args, options.AdditionalArguments);
        AddSystemIncludeDirectories(args, systemIncludeDirectories);

        return args.ToImmutable();
    }

    private void AddDefaults(
        ImmutableArray<string>.Builder args,
        TargetPlatform platform,
        bool isCPlusPlus,
        bool ignoreWarnings)
    {
        AddLanguageDefaults(args, platform, isCPlusPlus);

        if (ignoreWarnings)
        {
            AddIgnoreWarnings(args);
        }
    }

    private void AddLanguageDefaults(
        ImmutableArray<string>.Builder args,
        TargetPlatform platform,
        bool isCPlusPlus)
    {
        if (isCPlusPlus)
        {
            AddLanguageDefaultsCPlusPlus(args, platform);
        }
        else
        {
            AddLanguageDefaultsC(args, platform);
        }
    }

    private void AddLanguageDefaultsCPlusPlus(
        ImmutableArray<string>.Builder args,
        TargetPlatform platform)
    {
        args.Add("--language=c++");
        args.Add(platform.OperatingSystem == NativeOperatingSystem.Linux ? "--std=gnu++11" : "--std=c++11");
    }

    private void AddLanguageDefaultsC(
        ImmutableArray<string>.Builder args,
        TargetPlatform platform)
    {
        args.Add("--language=c");
        args.Add(platform.OperatingSystem == NativeOperatingSystem.Linux ? "--std=gnu11" : "--std=c11");
        args.Add("-fblocks");
        args.Add("-Wno-pragma-once-outside-header");
        args.Add("-fparse-all-comments");
    }

    private void AddIgnoreWarnings(ImmutableArray<string>.Builder args)
    {
        args.Add("-Wno-everything");
    }

    private void AddUserIncludeDirectories(
        ImmutableArray<string>.Builder args, ImmutableArray<string> includeDirectories)
    {
        if (includeDirectories.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var searchDirectory in includeDirectories)
        {
            var commandLineArg = "--include-directory=" + searchDirectory;
            args.Add(commandLineArg);
        }
    }

    private void AddDefines(ImmutableArray<string>.Builder args, ImmutableArray<string> defines)
    {
        if (defines.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var defineMacro in defines)
        {
            var commandLineArg = "--define-macro=" + defineMacro;
            args.Add(commandLineArg);
        }
    }

    private void AddTargetTriple(ImmutableArray<string>.Builder args, TargetPlatform platform)
    {
        var targetTripleString = $"--target={platform}";
        args.Add(targetTripleString);
    }

    private void AddAdditionalArgs(ImmutableArray<string>.Builder args, ImmutableArray<string> additionalArgs)
    {
        if (additionalArgs.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var arg in additionalArgs)
        {
            args.Add(arg);
        }
    }

    private void AddSystemIncludeDirectories(
        ImmutableArray<string>.Builder args,
        ImmutableArray<string> systemIncludeDirectories)
    {
        if (systemIncludeDirectories.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var directory in systemIncludeDirectories)
        {
            args.Add($"-isystem{directory}");
        }
    }
}
