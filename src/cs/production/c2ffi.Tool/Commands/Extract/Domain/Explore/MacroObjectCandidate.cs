// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore;

public class MacroObjectCandidate
{
    public string Name { get; init; } = string.Empty;

    public CLocation? Location { get; init; }

    public ImmutableArray<string> Tokens { get; init; } = ImmutableArray<string>.Empty;
}
