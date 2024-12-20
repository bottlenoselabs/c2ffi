// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace c2ffi;

[AttributeUsage(AttributeTargets.Assembly)]
internal sealed class ProjectInfoAttribute(string toolDescription) : Attribute
{
    public string ToolDescription { get; } = toolDescription;
}
