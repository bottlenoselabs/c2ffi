// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestFunctionPointerParameter(CFunctionPointerParameter functionPointerParameter)
{
    public string Name { get; } = functionPointerParameter.Name;

    public string TypeName { get; } = functionPointerParameter.Type.Name;

    public override string ToString()
    {
        return Name;
    }
}
