// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestFunctionPointer(CFunctionPointer functionPointer)
{
    public string Name { get; } = functionPointer.Name;

    public string CallingConvention { get; } = functionPointer.CallingConvention.ToString().ToLowerInvariant();

    public CTestType ReturnType { get; } = new(functionPointer.ReturnType);

    public ImmutableArray<CTestFunctionPointerParameter> Parameters { get; set; } = [
        ..functionPointer.Parameters
            .Select(x => new CTestFunctionPointerParameter(x))
    ];

    public override string ToString()
    {
        return Name;
    }
}
