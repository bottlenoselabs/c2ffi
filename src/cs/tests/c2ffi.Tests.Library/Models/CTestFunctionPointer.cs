// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestFunctionPointer
{
    public string Name { get; }

    public string CallingConvention { get; }

    public CTestType ReturnType { get; }

    public ImmutableArray<CTestFunctionPointerParameter> Parameters { get; set; }

    public CTestFunctionPointer(CFunctionPointer functionPointer)
    {
        Name = functionPointer.Name;
        CallingConvention = functionPointer.CallingConvention.ToString().ToLowerInvariant();
        ReturnType = new CTestType(functionPointer.ReturnType);
        Parameters = functionPointer.Parameters
            .Select(x => new CTestFunctionPointerParameter(x)).ToImmutableArray();
    }

    public override string ToString()
    {
        return Name;
    }
}
