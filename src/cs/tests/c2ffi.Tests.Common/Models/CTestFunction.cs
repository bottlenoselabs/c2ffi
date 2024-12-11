// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestFunction(CFunction function)
{
    public string Name { get; } = function.Name;

    public string CallingConvention { get; } = function.CallingConvention.ToString().ToLowerInvariant();

    public CTestType ReturnType { get; } = new CTestType(function.ReturnType);

    public ImmutableArray<CTestFunctionParameter> Parameters { get; } = function.Parameters
            .Select(x => new CTestFunctionParameter(x)).ToImmutableArray();

    public string? Comment { get; } = function.Comment;

    public override string ToString()
    {
        return Name;
    }
}
