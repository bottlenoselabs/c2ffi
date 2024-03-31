// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data;

namespace c2ffi.Tool.Commands.Extract.Input.Sanitized;

public sealed class ExtractTargetPlatformOptions
{
    public string OutputFilePath { get; init; } = string.Empty;

    public TargetPlatform TargetPlatform { get; init; } = TargetPlatform.Unknown;

    public ImmutableArray<string> UserIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> SystemIncludeDirectories { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> IgnoredIncludeFiles { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> MacroObjectDefines { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> AdditionalArguments { get; init; } = ImmutableArray<string>.Empty;

    public bool IsEnabledFindSystemHeaders { get; init; }

    public bool IsEnabledSystemDeclarations { get; init; }

    public bool IsEnabledOnlyExternalTopLevelCursors { get; init; }

    public ImmutableHashSet<string> OpaqueTypeNames { get; init; } = ImmutableHashSet<string>.Empty;

    public ImmutableHashSet<string> AllowedMacroObjects { get; init; } = ImmutableHashSet<string>.Empty;

    public ImmutableHashSet<string> AllowedVariables { get; init; } = ImmutableHashSet<string>.Empty;

    public override string ToString()
    {
        return $"{{ TargetPlatform: {TargetPlatform}, OutputFilePath: {OutputFilePath} }}";
    }
}
