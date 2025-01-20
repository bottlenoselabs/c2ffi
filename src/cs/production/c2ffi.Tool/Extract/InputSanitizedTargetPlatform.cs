// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using c2ffi.Data;

namespace c2ffi.Extract;

public sealed class InputSanitizedTargetPlatform
{
    private readonly ImmutableArray<Regex> _allowedNameRegexRegexes;

    private readonly ImmutableArray<Regex> _blockedNameRegexRegexes;

    public string OutputFilePath { get; init; } = string.Empty;

    public TargetPlatform TargetPlatform { get; init; } = TargetPlatform.Unknown;

    public ImmutableArray<string> UserIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> SystemIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> IgnoredIncludeFiles { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableDictionary<string, string> MacroObjectDefines { get; init; } = ImmutableDictionary<string, string>.Empty;

    public ImmutableArray<string> AdditionalArguments { get; init; } = ImmutableArray<string>.Empty;

    public bool IsEnabledFindSystemHeaders { get; init; }

    public InputSanitizedTargetPlatform(
        ImmutableArray<Regex> allowedNameRegexes,
        ImmutableArray<Regex> blockedNameRegexes)
    {
        _allowedNameRegexRegexes = allowedNameRegexes;
        _blockedNameRegexRegexes = blockedNameRegexes;
    }

    public override string ToString()
    {
        return $"{{ TargetPlatform: {TargetPlatform}, OutputFilePath: {OutputFilePath} }}";
    }

    public bool IsNameAllowed(string name)
    {
        bool isAllowed;
        if (_allowedNameRegexRegexes.IsDefaultOrEmpty)
        {
            isAllowed = true;
        }
        else
        {
            isAllowed = false;
            foreach (var regex in _allowedNameRegexRegexes)
            {
                if (regex.IsMatch(name))
                {
                    isAllowed = true;
                    break;
                }
            }
        }

        bool isBlocked;
        if (_blockedNameRegexRegexes.IsDefaultOrEmpty)
        {
            isBlocked = false;
        }
        else
        {
            isBlocked = false;
            foreach (var regex in _blockedNameRegexRegexes)
            {
                if (regex.IsMatch(name))
                {
                    isBlocked = true;
                    break;
                }
            }
        }

        return isAllowed && !isBlocked;
    }
}
