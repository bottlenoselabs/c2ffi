// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using JetBrains.Annotations;

namespace c2json.Tool.Commands.Merge;

[UsedImplicitly]
public class MergeAbstractSyntaxTreesCommand : Command
{
    public MergeAbstractSyntaxTreesCommand()
        : base(
            "merge",
            "Merge multiple target platform FFI (foreign function interface) `.json` abstract syntax tree (AST) into a cross-platform FFI `.json` AST.")
    {
        var directoryOption = new Option<string>(
            "--inputDirectoryPath", "The input directory where the multiple target platform FFI (foreign function interface) `.json` abstract syntax tree (AST) are located.")
        {
            IsRequired = true
        };
        AddOption(directoryOption);

        var fileOption = new Option<string>(
            "--outputFilePath", "The output file path of the cross-platform FFI (foreign function interface) `.json` abstract syntax tree (AST).");
        AddOption(fileOption);

        this.SetHandler(Main, directoryOption, fileOption);
    }

    private void Main(string inputDirectoryPath, string outputFilePath)
    {
        Console.WriteLine("Merge!");
    }
}
