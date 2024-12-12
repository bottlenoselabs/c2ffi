// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestFunctionParameter(CFunctionParameter functionParameter)
{
    public string Name { get; } = functionParameter.Name;

    public CTestType Type { get; } = new CTestType(functionParameter.Type);

    public override string ToString()
    {
        return Name;
    }
}
