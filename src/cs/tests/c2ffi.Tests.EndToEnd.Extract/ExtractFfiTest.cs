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

    public ImmutableArray<CTestFfiTargetPlatform> GetFfis(string configurationFilePath)
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
            var result = CreateTestFunction(function);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestFunction CreateTestFunction(CFunction value)
    {
        var parameters = CreateTestFunctionParameters(value.Parameters);

        var result = new CTestFunction
        {
            Name = value.Name,
#pragma warning disable CA1308
            CallingConvention = value.CallingConvention.ToString().ToLowerInvariant(),
#pragma warning restore CA1308
            ReturnTypeName = value.ReturnTypeInfo.Name,
            Parameters = parameters,
            Comment = value.Comment
        };
        return result;
    }

    private static ImmutableArray<CTestFunctionParameter> CreateTestFunctionParameters(
        ImmutableArray<CFunctionParameter> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestFunctionParameter>();

        foreach (var value in values)
        {
            var result = CreateTestFunctionParameter(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestFunctionParameter CreateTestFunctionParameter(CFunctionParameter value)
    {
        var result = new CTestFunctionParameter
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name
        };

        return result;
    }

    private static ImmutableDictionary<string, CTestEnum> CreateTestEnums(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestEnum>();

        foreach (var @enum in ffi.Enums.Values)
        {
            var result = CreateTestEnum(@enum);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestEnum CreateTestEnum(CEnum value)
    {
        var values = CreateTestEnumValues(value.Values);

        var result = new CTestEnum
        {
            Name = value.Name,
            IntegerType = value.IntegerTypeInfo.Name,
            Values = values
        };
        return result;
    }

    private static ImmutableArray<CTestEnumValue> CreateTestEnumValues(ImmutableArray<CEnumValue> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestEnumValue>();

        foreach (var value in values)
        {
            var result = CreateTestEnumValue(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestEnumValue CreateTestEnumValue(CEnumValue value)
    {
        var result = new CTestEnumValue
        {
            Name = value.Name,
            Value = value.Value
        };
        return result;
    }

    private static ImmutableDictionary<string, CTestRecord> CreateTestRecords(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestRecord>();

        foreach (var value in ffi.Records.Values)
        {
            var result = CreateTestRecord(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestRecord CreateTestRecord(CRecord value)
    {
        var name = value.Name;
        var fields = CreateTestRecordFields(value.Fields);

        var result = new CTestRecord
        {
            Name = name,
            SizeOf = value.SizeOf,
            AlignOf = value.AlignOf,
            Fields = fields,
            IsUnion = false
        };

        return result;
    }

    private static ImmutableArray<CTestRecordField> CreateTestRecordFields(ImmutableArray<CRecordField> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestRecordField>();

        foreach (var value in values)
        {
            var result = CreateTestRecordField(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestRecordField CreateTestRecordField(CRecordField value)
    {
        var result = new CTestRecordField
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name,
            OffsetOf = value.OffsetOf,
            SizeOf = value.TypeInfo.SizeOf!.Value
        };

        return result;
    }

    private ImmutableDictionary<string, CTestMacroObject> CreateTestMacroObjects(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestMacroObject>();

        foreach (var value in ffi.MacroObjects.Values)
        {
            var result = CreateMacroObject(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private CTestMacroObject CreateMacroObject(CMacroObject value)
    {
        var macroObject = new CTestMacroObject
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name,
            Value = value.Value
        };

        return macroObject;
    }

    private ImmutableDictionary<string, CTestTypeAlias> CreateTestTypeAliases(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestTypeAlias>();

        foreach (var value in ffi.TypeAliases.Values)
        {
            var result = CreateTestTypeAlias(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private CTestTypeAlias CreateTestTypeAlias(CTypeAlias value)
    {
        var typeAlias = new CTestTypeAlias
        {
            Name = value.Name,
            UnderlyingName = value.UnderlyingTypeInfo.Name,
            UnderlyingKind = value.UnderlyingTypeInfo.NodeKind.ToString()
        };

        return typeAlias;
    }

    private ImmutableDictionary<string, CTestFunctionPointer> CreateTestFunctionPointers(CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestFunctionPointer>();

        foreach (var value in ffi.FunctionPointers.Values)
        {
            var result = CreateTestFunctionPointer(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private CTestFunctionPointer CreateTestFunctionPointer(CFunctionPointer value)
    {
        var parameters = CreateTestFunctionPointerParameters(value.Parameters);

        var result = new CTestFunctionPointer
        {
            Name = value.Name,
            CallingConvention = "todo",
            ReturnTypeName = value.ReturnTypeInfo.Name,
            Parameters = parameters
        };

        return result;
    }

    private static ImmutableArray<CTestFunctionPointerParameter> CreateTestFunctionPointerParameters(
        ImmutableArray<CFunctionPointerParameter> values)
    {
        var builder = ImmutableArray.CreateBuilder<CTestFunctionPointerParameter>();

        foreach (var value in values)
        {
            var result = CreateTestFunctionPointerParameter(value);
            builder.Add(result);
        }

        return builder.ToImmutable();
    }

    private static CTestFunctionPointerParameter CreateTestFunctionPointerParameter(CFunctionPointerParameter value)
    {
        var functionPointerParameter = new CTestFunctionPointerParameter
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name
        };

        return functionPointerParameter;
    }

    private static ImmutableDictionary<string, CTestOpaqueType> CreateTestOpaqueTypes(
        CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestOpaqueType>();

        foreach (var value in ffi.OpaqueTypes.Values)
        {
            var result = CreateTestOpaqueType(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestOpaqueType CreateTestOpaqueType(COpaqueType value)
    {
        var opaqueType = new CTestOpaqueType
        {
            Name = value.Name
        };

        return opaqueType;
    }

    private static ImmutableDictionary<string, CTestVariable> CreateTestVariables(
        CFfiTargetPlatform ffi)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, CTestVariable>();

        foreach (var value in ffi.Variables.Values)
        {
            var result = CreateTestVariable(value);
            builder.Add(result.Name, result);
        }

        return builder.ToImmutable();
    }

    private static CTestVariable CreateTestVariable(CVariable value)
    {
        var variable = new CTestVariable
        {
            Name = value.Name,
            TypeName = value.TypeInfo.Name
        };

        return variable;
    }
}
