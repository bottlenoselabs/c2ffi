// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace c2ffi.Tool;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ProjectInfoAttribute(string toolDescription) : Attribute
{
    public string ToolDescription { get; } = toolDescription;
}
