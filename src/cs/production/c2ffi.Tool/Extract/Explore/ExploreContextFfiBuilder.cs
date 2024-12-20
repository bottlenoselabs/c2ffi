// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Extract.Parse;

namespace c2ffi.Extract.Explore;

internal sealed class ExploreContextFfiBuilder
{
    private readonly List<CVariable> _variables = [];
    private readonly List<CFunction> _functions = [];
    private readonly List<CRecord> _records = [];
    private readonly List<CEnum> _enums = [];
    private readonly List<CTypeAlias> _typeAliases = [];
    private readonly List<COpaqueType> _opaqueTypes = [];
    private readonly List<CFunctionPointer> _functionPointers = [];
    private readonly List<CMacroObject> _macroObjects = [];

    public CFfiTargetPlatform GetFfi(ParseContext context)
    {
        var variables = CollectVariables();
        var functions = CollectFunctions();
        var records = CollectRecords();
        var enums = CollectEnums();
        var typeAliases = CollectTypeAliases();
        var opaqueTypes = CollectOpaqueTypes();
        var functionPointers = CollectFunctionPointers();
        var macroObjects = CollectMacroObjects();

        var result = new CFfiTargetPlatform
        {
            FileName = context.FilePath,
            PlatformRequested = context.TargetPlatformRequested,
            PlatformActual = context.TargetPlatformActual,
            ClangArguments = context.Arguments,
            SystemIncludeDirectories = context.SystemIncludeDirectories,
            UserIncludeDirectories = context.InputSanitized.UserIncludeDirectories,
            PointerSize = context.PointerSize,
            Variables = variables,
            Functions = functions,
            Records = records,
            Enums = enums,
            TypeAliases = typeAliases,
            OpaqueTypes = opaqueTypes,
            FunctionPointers = functionPointers,
            MacroObjects = macroObjects,
        };

        return result;
    }

    public void AddNode(CNode node)
    {
#pragma warning disable IDE0010
        switch (node.NodeKind)
#pragma warning restore IDE0010
        {
            case CNodeKind.Variable:
                AddVariable((CVariable)node);
                break;
            case CNodeKind.Function:
                AddFunction((CFunction)node);
                break;
            case CNodeKind.Struct:
            case CNodeKind.Union:
                AddRecord((CRecord)node);
                break;
            case CNodeKind.Enum:
                AddEnum((CEnum)node);
                break;
            case CNodeKind.TypeAlias:
                AddTypeAlias((CTypeAlias)node);
                break;
            case CNodeKind.OpaqueType:
                AddOpaqueType((COpaqueType)node);
                break;
            case CNodeKind.FunctionPointer:
                AddFunctionPointer((CFunctionPointer)node);
                break;
            case CNodeKind.MacroObject:
                AddMacroObject((CMacroObject)node);
                break;
            case CNodeKind.Pointer:
            case CNodeKind.Array:
            case CNodeKind.Primitive:
                break;
            default:
                var up = new NotImplementedException(
                    $"Could not add a C node of kind '{node.NodeKind}'.");
                throw up;
        }
    }

    private void AddVariable(CVariable node)
    {
        _variables.Add(node);
    }

    private void AddFunction(CFunction node)
    {
        _functions.Add(node);
    }

    private void AddRecord(CRecord node)
    {
        _records.Add(node);
    }

    private void AddEnum(CEnum node)
    {
        _enums.Add(node);
    }

    private void AddTypeAlias(CTypeAlias node)
    {
        _typeAliases.Add(node);
    }

    private void AddOpaqueType(COpaqueType node)
    {
        _opaqueTypes.Add(node);
    }

    private void AddFunctionPointer(CFunctionPointer node)
    {
        _functionPointers.Add(node);
    }

    private void AddMacroObject(CMacroObject node)
    {
        _macroObjects.Add(node);
    }

    private ImmutableSortedDictionary<string, CVariable> CollectVariables()
    {
        var variables = _variables
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return variables;
    }

    private ImmutableSortedDictionary<string, CFunction> CollectFunctions()
    {
        var functions = _functions
            .ToImmutableSortedDictionary(x => x.Name, x => x);

        return functions;
    }

    private ImmutableSortedDictionary<string, CRecord> CollectRecords()
    {
        var records = _records
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return records;
    }

    private ImmutableSortedDictionary<string, CEnum> CollectEnums()
    {
        var enums = _enums
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return enums;
    }

    private ImmutableSortedDictionary<string, CTypeAlias> CollectTypeAliases()
    {
        var typeAliases = _typeAliases
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return typeAliases;
    }

    private ImmutableSortedDictionary<string, COpaqueType> CollectOpaqueTypes()
    {
        var opaqueTypes = _opaqueTypes
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return opaqueTypes;
    }

    private ImmutableSortedDictionary<string, CFunctionPointer> CollectFunctionPointers()
    {
        var functionPointers = _functionPointers
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return functionPointers;
    }

    private ImmutableSortedDictionary<string, CMacroObject> CollectMacroObjects()
    {
        var macroObjects = _macroObjects
            .ToImmutableSortedDictionary(x => x.Name, x => x);
        return macroObjects;
    }
}
