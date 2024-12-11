// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using c2ffi.Data;
using c2ffi.Data.Serialization;
using c2ffi.Tests.Library;
using c2ffi.Tests.Library.Helpers;
using c2ffi.Tool.Commands.Extract;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tests.EndToEnd.Extract;

[PublicAPI]
[ExcludeFromCodeCoverage]
public abstract class ExtractFfiTest
{
    private readonly IFileSystem _fileSystem;
    private readonly IPath _path;
    private readonly IDirectory _directory;
    private readonly IFile _file;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly ExtractFfiTool _tool;

    protected ExtractFfiTest()
    {
        var services = TestHost.Services;
        _fileSystem = services.GetService<IFileSystem>()!;
        _path = _fileSystem.Path;
        _directory = _fileSystem.Directory;
        _file = _fileSystem.File;
        _fileSystemHelper = services.GetService<FileSystemHelper>()!;
        _tool = services.GetService<ExtractFfiTool>()!;
    }

    public ImmutableArray<CTestFfiTargetPlatform> GetTargetPlatformFfis(string configurationFilePath)
    {
        var fullConfigurationFilePath = _fileSystemHelper.GetFullFilePath(configurationFilePath);
        var oldFilePaths = GetFfiFilePaths(fullConfigurationFilePath);
        DeleteFiles(oldFilePaths);
        RunTool(fullConfigurationFilePath);
        var filePaths = GetFfiFilePaths(fullConfigurationFilePath);
        return ReadFfis(filePaths);
    }

    private ImmutableArray<CTestFfiTargetPlatform> ReadFfis(IEnumerable<string> filePaths)
    {
        var builder = ImmutableArray.CreateBuilder<CTestFfiTargetPlatform>();
        foreach (var filePath in filePaths)
        {
            var ffi = ReadFfi(filePath);
            builder.Add(ffi);
        }

        return builder.ToImmutable();
    }

    private void RunTool(string configurationFilePath)
    {
        _tool.Run(configurationFilePath);
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

    private void DeleteFiles(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            _file.Delete(filePath);
        }
    }

    private CTestFfiTargetPlatform ReadFfi(string filePath)
    {
        var ffi = Json.ReadFfiTargetPlatform(_fileSystem, filePath);

        var functions = CreateTestFunctions(ffi);
        var enums = CreateTestEnums(ffi);
        var structs = CreateTestRecords(ffi);
        var macroObjects = CreateTestMacroObjects(ffi);
        var typeAliases = CreateTestTypeAliases(ffi);
        var functionPointers = CreateTestFunctionPointers(ffi);
        var opaqueDataTypes = CreateTestOpaqueTypes(ffi);
        var variables = CreateTestVariables(ffi);

        var result = new CTestFfiTargetPlatform(
            ffi.PlatformRequested.ToString(),
            ffi.PlatformActual.ToString(),
            ffi.PointerSize,
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

    private static ImmutableDictionary<string, CTestFunction> CreateTestFunctions(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestFunction>();

        foreach (var function in ffi.Functions.Values)
        {
            var result = new CTestFunction(function);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, CTestEnum> CreateTestEnums(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestEnum>();

        foreach (var @enum in ffi.Enums.Values)
        {
            var result = new CTestEnum(@enum);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, CTestRecord> CreateTestRecords(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestRecord>();

        foreach (var value in ffi.Records.Values)
        {
            var result = new CTestRecord(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private ImmutableDictionary<string, CTestMacroObject> CreateTestMacroObjects(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestMacroObject>();

        foreach (var value in ffi.MacroObjects.Values)
        {
            var result = new CTestMacroObject(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private ImmutableDictionary<string, CTestTypeAlias> CreateTestTypeAliases(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestTypeAlias>();

        foreach (var value in ffi.TypeAliases.Values)
        {
            var result = new CTestTypeAlias(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private ImmutableDictionary<string, CTestFunctionPointer> CreateTestFunctionPointers(CFfiTargetPlatform ffi)
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
        CFfiTargetPlatform ffi)
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
        CFfiTargetPlatform ffi)
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
