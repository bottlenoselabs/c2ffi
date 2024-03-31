// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Data.Serialization;
using c2ffi.Tests.Library;
using c2ffi.Tests.Library.Helpers;
using c2ffi.Tests.Library.Models;
using c2ffi.Tool.Commands.Merge;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tests.EndToEnd.Merge;

[PublicAPI]
[ExcludeFromCodeCoverage]
public abstract class MergeFfisTest
{
    private readonly IFileSystem _fileSystem;
    private readonly IPath _path;
    private readonly IDirectory _directory;
    private readonly IFile _file;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly MergeFfisTool _tool;

    protected MergeFfisTest()
    {
        var services = TestHost.Services;
        _fileSystem = services.GetService<IFileSystem>()!;
        _path = _fileSystem.Path;
        _directory = _fileSystem.Directory;
        _file = _fileSystem.File;
        _fileSystemHelper = services.GetService<FileSystemHelper>()!;
        _tool = services.GetService<MergeFfisTool>()!;
    }

    public CTestFfiCrossPlatform GetFfi(string ffiDirectoryPath)
    {
        var fullFfiDirectoryPath = _fileSystemHelper.GetFullDirectoryPath(ffiDirectoryPath);
        try
        {
            _fileSystem.Directory.Delete(_fileSystem.Path.Combine(fullFfiDirectoryPath, "../ffi-x"), true);
        }
        catch (DirectoryNotFoundException)
        {
        }

        var fullOutputFilePath = _fileSystem.Path.Combine(fullFfiDirectoryPath, "../ffi-x/cross-platform.json");
        RunTool(fullFfiDirectoryPath, fullOutputFilePath);
        return ReadFfi(fullOutputFilePath);
    }

    private void RunTool(string inputDirectoryPath, string outputFilePath)
    {
        _tool.Run(inputDirectoryPath, outputFilePath);
    }

    private void DeleteFiles(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            _file.Delete(filePath);
        }
    }

    private IEnumerable<string> GetFfiFilePaths(string configurationFilePath)
    {
        var directoryPath = _path.GetDirectoryName(configurationFilePath)!;
        var ffiDirectoryPath = _path.Combine(directoryPath, "ffi");
        if (!_directory.Exists(ffiDirectoryPath))
        {
            return Array.Empty<string>();
        }

        return _directory.EnumerateFiles(ffiDirectoryPath);
    }

    private CTestFfiCrossPlatform ReadFfi(string filePath)
    {
        var ffi = Json.ReadFfiCrossPlatform(_fileSystem, filePath);

        var functions = CreateTestFunctions(ffi);
        var enums = CreateTestEnums(ffi);
        var structs = CreateTestRecords(ffi);
        var macroObjects = CreateTestMacroObjects(ffi);
        var typeAliases = CreateTestTypeAliases(ffi);
        var functionPointers = CreateTestFunctionPointers(ffi);
        var opaqueDataTypes = CreateTestOpaqueTypes(ffi);
        var variables = CreateTestVariables(ffi);

        var result = new CTestFfiCrossPlatform(
            functions,
            enums,
            structs,
            macroObjects,
            typeAliases,
            functionPointers,
            opaqueDataTypes,
            variables);
        return result;
    }

    private static ImmutableDictionary<string, CTestFunction> CreateTestFunctions(CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestFunction>();

        foreach (var function in ffi.Functions.Values)
        {
            var result = new CTestFunction(function);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, CTestEnum> CreateTestEnums(CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestEnum>();

        foreach (var @enum in ffi.Enums.Values)
        {
            var result = new CTestEnum(@enum);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, CTestRecord> CreateTestRecords(CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestRecord>();

        foreach (var value in ffi.Records.Values)
        {
            var result = new CTestRecord(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<CTestRecordField> CreateTestRecordFields(ImmutableArray<CRecordField> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestRecordField>();

        foreach (var value in values)
        {
            var result = new CTestRecordField(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private ImmutableDictionary<string, CTestMacroObject> CreateTestMacroObjects(CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestMacroObject>();

        foreach (var value in ffi.MacroObjects.Values)
        {
            var result = new CTestMacroObject(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private ImmutableDictionary<string, CTestTypeAlias> CreateTestTypeAliases(CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestTypeAlias>();

        foreach (var value in ffi.TypeAliases.Values)
        {
            var result = new CTestTypeAlias(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private ImmutableDictionary<string, CTestFunctionPointer> CreateTestFunctionPointers(CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestFunctionPointer>();

        foreach (var value in ffi.FunctionPointers.Values)
        {
            var result = new CTestFunctionPointer(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, CTestOpaqueType> CreateTestOpaqueTypes(
        CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestOpaqueType>();

        foreach (var value in ffi.OpaqueTypes.Values)
        {
            var result = new CTestOpaqueType(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, CTestVariable> CreateTestVariables(
        CFfiCrossPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestVariable>();

        foreach (var value in ffi.Variables.Values)
        {
            var result = new CTestVariable(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }
}
