// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo("c2ffi.Tool")]

#pragma warning disable CA1724

namespace c2ffi.Data.Serialization;

[PublicAPI]
public static class Json
{
    private static readonly JsonSerializerContextCFfiTargetPlatform ContextTargetPlatform =
        new(new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });

    private static readonly JsonSerializerContextCFfiCrossPlatform ContextCrossPlatform = new(new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    });

    public static CFfiTargetPlatform ReadFfiTargetPlatform(
        IFileSystem fileSystem, string filePath)
    {
        var fullFilePath = fileSystem.Path.GetFullPath(filePath);
        var fileContents = fileSystem.File.ReadAllText(fullFilePath);
        var result = JsonSerializer.Deserialize(fileContents, ContextTargetPlatform.CFfiTargetPlatform)!;
        FillNamesTargetPlatform(result);
        return result;
    }

    public static CFfiCrossPlatform ReadFfiCrossPlatform(
        IFileSystem fileSystem,
        string filePath)
    {
        var fullFilePath = fileSystem.Path.GetFullPath(filePath);
        var fileContents = fileSystem.File.ReadAllText(fullFilePath);
        var result = JsonSerializer.Deserialize(fileContents, ContextCrossPlatform.CFfiCrossPlatform)!;
        FillNamesCrossPlatform(result);
        return result;
    }

    public static void WriteFfiTargetPlatform(
        IFileSystem fileSystem,
        string filePath,
        CFfiTargetPlatform ffi)
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
            _ = Directory.CreateDirectory(outputDirectory);
        }

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        var fileContents = JsonSerializer.Serialize(ffi, ContextTargetPlatform.Options);

        using var fileStream = fileSystem.File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    public static void WriteFfiCrossPlatform(
        IFileSystem fileSystem,
        string filePath,
        CFfiCrossPlatform ffi)
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
            _ = fileSystem.Directory.CreateDirectory(outputDirectory);
        }

        if (fileSystem.File.Exists(fullFilePath))
        {
            fileSystem.File.Delete(fullFilePath);
        }

        var fileContents = JsonSerializer.Serialize(ffi, ContextCrossPlatform.Options);

        using var fileStream = fileSystem.File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    private static void FillNamesTargetPlatform(CFfiTargetPlatform ffi)
    {
        foreach (var keyValuePair in ffi.MacroObjects)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Variables)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Functions)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Records)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Enums)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.TypeAliases)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.OpaqueTypes)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.FunctionPointers)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }
    }

    private static void FillNamesCrossPlatform(CFfiCrossPlatform ffi)
    {
        foreach (var keyValuePair in ffi.MacroObjects)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Variables)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Functions)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Records)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.Enums)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.TypeAliases)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.OpaqueTypes)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in ffi.FunctionPointers)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }
    }
}
