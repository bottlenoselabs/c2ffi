// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;

namespace c2ffi.Tests.Library.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class CTestMacroObject
{
    public string Name { get; }

    public string TypeName { get; }

    public string Value { get; }

    public CTestMacroObject(CMacroObject macroObject)
    {
        Name = macroObject.Name;
        TypeName = macroObject.TypeInfo.Name;
        Value = macroObject.Value;
    }
}
