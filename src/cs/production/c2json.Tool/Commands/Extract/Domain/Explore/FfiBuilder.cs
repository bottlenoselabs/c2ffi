// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2json.Data;
using c2json.Data.Nodes;
using c2json.Tool.Commands.Extract.Domain.Parse;

namespace c2json.Tool.Commands.Extract.Domain.Explore;

public sealed class FfiBuilder
{
    private readonly List<CArray> _arrays = new();
    private readonly List<CPointer> _pointers = new();
    private readonly List<CPrimitive> _primitives = new();

    private readonly List<CVariable> _variables = new();
    private readonly List<CFunction> _functions = new();
    private readonly List<CRecord> _records = new();
    private readonly List<CEnum> _enums = new();
    private readonly List<CTypeAlias> _typeAliases = new();
    private readonly List<COpaqueType> _opaqueTypes = new();
    private readonly List<CFunctionPointer> _functionPointers = new();
    private readonly List<CEnumConstant> _enumConstants = new();

    public CFfiTargetPlatform GetFfi(ParseContext context)
    {
        // var macroObjects = CollectMacroObjects(context);
        var variables = CollectVariables();
        var functions = CollectFunctions();
        var records = CollectRecords();
        var enums = CollectEnums();
        var typeAliases = CollectTypeAliases();
        var opaqueTypes = CollectOpaqueTypes();
        var functionPointers = CollectFunctionPointers();
        var enumConstants = CollectEnumConstants();

        var result = new CFfiTargetPlatform
        {
            FileName = context.FilePath,
            PlatformRequested = context.TargetPlatformRequested,
            PlatformActual = context.TargetPlatformActual,
            // MacroObjects = macroObjects.ToImmutableDictionary(x => x.Name),
            Variables = variables,
            Functions = functions,
            Records = records,
            Enums = enums,
            TypeAliases = typeAliases,
            OpaqueTypes = opaqueTypes,
            FunctionPointers = functionPointers,
            EnumConstants = enumConstants
        };

        return result;
    }

    public void AddNode(CNode node)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (node.NodeKind)
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
            case CNodeKind.Pointer:
                AddPointer((CPointer)node);
                break;
            case CNodeKind.Array:
                AddArray((CArray)node);
                break;
            case CNodeKind.EnumConstant:
                AddEnumConstant((CEnumConstant)node);
                break;
            case CNodeKind.Primitive:
                AddPrimitive((CPrimitive)node);
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

    private void AddEnumConstant(CEnumConstant node)
    {
        _enumConstants.Add(node);
    }

    private void AddArray(CArray node)
    {
        _arrays.Add(node);
    }

    private void AddPointer(CPointer node)
    {
        _pointers.Add(node);
    }

    private void AddPrimitive(CPrimitive node)
    {
        _primitives.Add(node);
    }

    private ImmutableDictionary<string, CVariable> CollectVariables()
    {
        _variables.Sort();
        var variables = _variables.ToImmutableDictionary(x => x.Name);
        return variables;
    }

    private ImmutableDictionary<string, CFunction> CollectFunctions()
    {
        _functions.Sort();
        var functions = _functions.ToImmutableDictionary(x => x.Name);
        return functions;
    }

    private ImmutableDictionary<string, CRecord> CollectRecords()
    {
        _records.Sort();
        var records = _records.ToImmutableDictionary(x => x.Name);
        return records;
    }

    private ImmutableDictionary<string, CEnum> CollectEnums()
    {
        _enums.Sort();
        var enums = _enums.ToImmutableDictionary(x => x.Name);
        return enums;
    }

    private ImmutableDictionary<string, CTypeAlias> CollectTypeAliases()
    {
        _typeAliases.Sort();
        var typeAliases = _typeAliases.ToImmutableDictionary(x => x.Name);
        return typeAliases;
    }

    private ImmutableDictionary<string, COpaqueType> CollectOpaqueTypes()
    {
        _opaqueTypes.Sort();
        var opaqueTypes = _opaqueTypes.ToImmutableDictionary(x => x.Name);
        return opaqueTypes;
    }

    private ImmutableDictionary<string, CFunctionPointer> CollectFunctionPointers()
    {
        _functionPointers.Sort();
        var functionPointers = _functionPointers.ToImmutableDictionary(x => x.Name);
        return functionPointers;
    }

    private ImmutableDictionary<string, CEnumConstant> CollectEnumConstants()
    {
        _enumConstants.Sort();
        var enumConstants = _enumConstants.ToImmutableDictionary(x => x.Name);
        return enumConstants;
    }
}
