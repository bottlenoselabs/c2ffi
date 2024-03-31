// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestFunction
{
    public string Name { get; }

    public string CallingConvention { get; }

    public string ReturnTypeName { get; }

    public ImmutableArray<CTestFunctionParameter> Parameters { get; }

    public string? Comment { get; }

    public CTestFunction(CFunction function)
    {
        Name = function.Name;
        CallingConvention = function.CallingConvention.ToString().ToLowerInvariant();
        ReturnTypeName = function.ReturnTypeInfo.Name;
        Parameters = function.Parameters
            .Select(x => new CTestFunctionParameter(x)).ToImmutableArray();
        Comment = function.Comment;
    }

    public override string ToString()
    {
        return Name;
    }
}
