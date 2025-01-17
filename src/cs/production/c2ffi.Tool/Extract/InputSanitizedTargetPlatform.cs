// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using c2ffi.Data;

namespace c2ffi.Extract;

public sealed class InputSanitizedTargetPlatform
{
    public string OutputFilePath { get; init; } = string.Empty;

    public TargetPlatform TargetPlatform { get; init; } = TargetPlatform.Unknown;

    public ImmutableArray<string> UserIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> SystemIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> IgnoredIncludeFiles { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableDictionary<string, string> MacroObjectDefines { get; init; } = ImmutableDictionary<string, string>.Empty;

    public ImmutableArray<string> AdditionalArguments { get; init; } = ImmutableArray<string>.Empty;

    public bool IsEnabledFindSystemHeaders { get; init; }

    public ImmutableHashSet<string> IncludedNames { get; init; } = ImmutableHashSet<string>.Empty;

    public ImmutableArray<Regex> IgnoreNameRegexes { get; init; } = ImmutableArray<Regex>.Empty;

    public override string ToString()
    {
        return $"{{ TargetPlatform: {TargetPlatform}, OutputFilePath: {OutputFilePath} }}";
    }
}
