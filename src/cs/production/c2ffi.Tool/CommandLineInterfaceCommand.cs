// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using c2ffi.Tool.Commands.Extract;
using c2ffi.Tool.Commands.Merge;
using JetBrains.Annotations;

namespace c2ffi.Tool;

[ExcludeFromCodeCoverage]
[UsedImplicitly]
internal sealed class CommandLineInterfaceCommand : RootCommand
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
