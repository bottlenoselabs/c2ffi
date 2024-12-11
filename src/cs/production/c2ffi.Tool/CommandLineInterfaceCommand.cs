// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace c2ffi;

[ExcludeFromCodeCoverage]
[UsedImplicitly]
internal sealed class CommandLineInterfaceCommand : RootCommand
{
    public CommandLineInterfaceCommand(
        Extract.Command extractCommand,
        Merge.Command mergeCommand)
        : base(GetDescription())
    {
        AddCommand(extractCommand);
        AddCommand(mergeCommand);
    }

    private static string GetDescription()
    {
        var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<ProjectInfoAttribute>();
        return attribute!.ToolDescription;
    }
}
