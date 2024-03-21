// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("c2json.Tool")]

#pragma warning disable CA1724

namespace c2json.Data.Serialization;

[PublicAPI]
public static class Json
{
    private static readonly JsonSerializerContextCAbstractSyntaxTreeTargetPlatform ContextTargetPlatform =
        new(new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });

    private static readonly JsonSerializerContextCAbstractSyntaxTreeCrossPlatform ContextCrossPlatform = new(new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    });

    public static CAbstractSyntaxTreeTargetPlatform ReadAbstractSyntaxTreeTargetPlatform(
        IFileSystem fileSystem, string filePath)
    {
        var fullFilePath = fileSystem.Path.GetFullPath(filePath);
        var fileContents = fileSystem.File.ReadAllText(fullFilePath);
        var result = JsonSerializer.Deserialize(fileContents, ContextTargetPlatform.CAbstractSyntaxTreeTargetPlatform)!;
        FillNamesTargetPlatform(result);
        return result;
    }

    public static CAbstractSyntaxTreeCrossPlatform ReadAbstractSyntaxTreeCrossPlatform(
        IFileSystem fileSystem,
        string filePath)
    {
        var fullFilePath = fileSystem.Path.GetFullPath(filePath);
        var fileContents = fileSystem.File.ReadAllText(fullFilePath);
        var result = JsonSerializer.Deserialize(fileContents, ContextCrossPlatform.CAbstractSyntaxTreeCrossPlatform)!;
        FillNamesCrossPlatform(result);
        return result;
    }

    public static void WriteAbstractSyntaxTreeTargetPlatform(
        IFileSystem fileSystem,
        string filePath,
        CAbstractSyntaxTreeTargetPlatform abstractSyntaxTree)
    {
        var fullFilePath = fileSystem.Path.GetFullPath(filePath);

        var outputDirectory = fileSystem.Path.GetDirectoryName(fullFilePath)!;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = AppContext.BaseDirectory;
            fullFilePath = fileSystem.Path.Combine(Environment.CurrentDirectory, fullFilePath);
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        var fileContents = JsonSerializer.Serialize(abstractSyntaxTree, ContextTargetPlatform.Options);

        using var fileStream = fileSystem.File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    public static void WriteAbstractSyntaxTreeCrossPlatform(
        IFileSystem fileSystem,
        string filePath,
        CAbstractSyntaxTreeCrossPlatform abstractSyntaxTree)
    {
        var fullFilePath = fileSystem.Path.GetFullPath(filePath);

        var outputDirectory = fileSystem.Path.GetDirectoryName(fullFilePath)!;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = AppContext.BaseDirectory;
            fullFilePath = fileSystem.Path.Combine(Environment.CurrentDirectory, fullFilePath);
        }

        if (!fileSystem.Directory.Exists(outputDirectory))
        {
            fileSystem.Directory.CreateDirectory(outputDirectory);
        }

        if (fileSystem.File.Exists(fullFilePath))
        {
            fileSystem.File.Delete(fullFilePath);
        }

        var fileContents = JsonSerializer.Serialize(abstractSyntaxTree, ContextCrossPlatform.Options);

        using var fileStream = fileSystem.File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    private static void FillNamesTargetPlatform(CAbstractSyntaxTreeTargetPlatform abstractSyntaxTree)
    {
        foreach (var keyValuePair in abstractSyntaxTree.MacroObjects)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Variables)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Functions)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Records)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Enums)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.EnumConstants)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.TypeAliases)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.OpaqueTypes)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.FunctionPointers)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }
    }

    private static void FillNamesCrossPlatform(CAbstractSyntaxTreeCrossPlatform abstractSyntaxTree)
    {
        foreach (var keyValuePair in abstractSyntaxTree.MacroObjects)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Variables)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Functions)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Records)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Enums)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.EnumConstants)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.TypeAliases)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.OpaqueTypes)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.FunctionPointers)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }
    }
}
