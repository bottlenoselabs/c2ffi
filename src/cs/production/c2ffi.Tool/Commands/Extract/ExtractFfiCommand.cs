// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using JetBrains.Annotations;

namespace c2ffi.Tool.Commands.Extract;

[UsedImplicitly]
internal sealed class ExtractFfiCommand : Command
{
    private readonly ExtractFfiTool _tool;

    public ExtractFfiCommand(ExtractFfiTool tool)
        : base(
            "extract",
            "Extract the FFI (foreign function interface) of a C library given a header `.h` file.")
    {
        _tool = tool;

        var configurationFilePathOption = ConfigurationFilePathOption();
        AddOption(configurationFilePathOption);
        var clangFilePathOption = ClangFilePathOption();
        AddOption(clangFilePathOption);

        this.SetHandler(
            Main,
            configurationFilePathOption,
            clangFilePathOption);
    }

    private Option<string> ConfigurationFilePathOption()
    {
        var option = new Option<string>(
            ["--config", "--configFilePath"], "The file path to configure extraction of FFI `.json` files.");
        option.SetDefaultValue("config.json");
        return option;
    }

    private Option<string> ClangFilePathOption()
    {
        var option = new Option<string>(
            ["--clang", "--clangFilePath"], "The file path to the native libclang library.");
        option.SetDefaultValue(string.Empty);
        return option;
    }

    private void Main(string configurationFilePath, string clangFilePath)
    {
        _tool.Run(configurationFilePath, clangFilePath);
    }
}
