// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using c2json.Tool.Commands.Extract;
using c2json.Tool.Commands.Merge;

namespace c2json.Tool;

[ExcludeFromCodeCoverage]
public sealed class CommandLineInterfaceCommand : RootCommand
{
    public CommandLineInterfaceCommand(
        ExtractFfiCommand extractFfiCommand,
        MergeFfisCommand mergeFfisCommand)
        : base(GetDescription())
    {
        AddCommand(extractFfiCommand);
        AddCommand(mergeFfisCommand);
    }

    private static string GetDescription()
    {
        var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<ProjectInfoAttribute>();
        return attribute!.ToolDescription;
    }
}
