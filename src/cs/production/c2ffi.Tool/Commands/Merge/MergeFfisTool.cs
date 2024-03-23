// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Data.Serialization;
using c2ffi.Tool.Commands.Merge.Input;
using c2ffi.Tool.Commands.Merge.Input.Sanitized;
using c2ffi.Tool.Commands.Merge.Input.Unsanitized;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Merge;

public sealed partial class MergeFfisTool
{
    private readonly ILogger<MergeFfisTool> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly MergeInputSanitizer _mergeInputSanitizer;

    private readonly List<CEnum> _enums = new();
    private readonly List<CVariable> _variables = new();
    private readonly List<COpaqueType> _opaqueTypes = new();
    private readonly List<CFunction> _functions = new();
    private readonly List<CRecord> _records = new();
    private readonly List<CFunctionPointer> _functionPointers = new();
    private readonly List<CMacroObject> _macroObjects = new();
    private readonly List<CTypeAlias> _typeAliases = new();
    private readonly List<CEnumConstant> _enumConstants = new();

    private sealed class CNodeWithTargetPlatform
    {
        public readonly CNode Node;
        public readonly TargetPlatform TargetPlatform;

        public CNodeWithTargetPlatform(CNode node, TargetPlatform targetPlatform)
        {
            Node = node;
            TargetPlatform = targetPlatform;
        }
    }

    public MergeFfisTool(
        ILogger<MergeFfisTool> logger,
        IFileSystem fileSystem,
        MergeInputSanitizer mergeInputSanitizer)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _mergeInputSanitizer = mergeInputSanitizer;
    }

    public void Run(string inputDirectoryPath, string outputFilePath)
    {
        var options = GetOptions(inputDirectoryPath, outputFilePath);
        var platformFfis =
            GetPlatformFfis(options.InputFilePaths);
        var platforms = platformFfis.
            Select(x => x.PlatformRequested).ToImmutableArray();
        var platformNodesByName = GetPlatformNodesByName(platformFfis);
        var ffi = CreateCrossPlatformFfi(platforms, platformNodesByName);

        Json.WriteFfiCrossPlatform(_fileSystem, options.OutputFilePath, ffi);
        LogWriteAbstractSyntaxTreeSuccess(string.Join(", ", platforms), options.OutputFilePath);
    }

    private MergeOptions GetOptions(string inputDirectoryPath, string outputFilePath)
    {
        var unsanitizedOptions = new UnsanitizedMergeOptions
        {
            InputDirectoryPath = inputDirectoryPath,
            OutputFilePath = outputFilePath
        };
        return _mergeInputSanitizer.Sanitize(unsanitizedOptions);
    }

    private CFfiCrossPlatform CreateCrossPlatformFfi(
        ImmutableArray<TargetPlatform> platforms,
        ImmutableSortedDictionary<string, ImmutableArray<CNodeWithTargetPlatform>> platformNodesByName)
    {
        var result = new CFfiCrossPlatform();

        foreach (var (name, nodes) in platformNodesByName)
        {
            BuildCrossPlatformNodes(platforms, nodes, name);
        }

        result.Platforms = platforms.Sort(
            (a, b) =>
                string.Compare(a.ClangTargetTriple, b.ClangTargetTriple, StringComparison.Ordinal));
        result.Enums = _enums.ToImmutableDictionary(x => x.Name);
        result.Variables = _variables.ToImmutableDictionary(x => x.Name);
        result.OpaqueTypes = _opaqueTypes.ToImmutableDictionary(x => x.Name);
        result.Functions = _functions.ToImmutableDictionary(x => x.Name);
        result.Records = _records.ToImmutableDictionary(x => x.Name);
        result.FunctionPointers = _functionPointers.ToImmutableDictionary(x => x.Name);
        result.MacroObjects = _macroObjects.ToImmutableDictionary(x => x.Name);
        result.TypeAliases = _typeAliases.ToImmutableDictionary(x => x.Name);
        result.EnumConstants = _enumConstants.ToImmutableDictionary(x => x.Name);
        return result;
    }

    private void AddCrossPlatformNode(CNodeWithTargetPlatform nodeWithTargetPlatform)
    {
        var node = nodeWithTargetPlatform.Node;

        if (node is CNodeWithLocation nodeWithLocation)
        {
            nodeWithLocation.Location = null;
        }

        switch (node)
        {
            case CEnum @enum:
                ClearLocationForTypeInfo(@enum.IntegerTypeInfo);
                _enums.Add(@enum);
                break;
            case CVariable variable:
                _variables.Add(variable);
                break;
            case COpaqueType opaqueType:
                _opaqueTypes.Add(opaqueType);
                break;
            case CFunction function:
                ClearLocationForTypeInfo(function.ReturnTypeInfo);
                foreach (var parameter in function.Parameters)
                {
                    parameter.Location = null;
                    ClearLocationForTypeInfo(parameter.TypeInfo);
                }

                _functions.Add(function);
                break;
            case CRecord record:
                foreach (var field in record.Fields)
                {
                    field.Location = null;
                    ClearLocationForTypeInfo(field.TypeInfo);
                }

                _records.Add(record);
                break;
            case CFunctionPointer functionPointer:
                ClearLocationForTypeInfo(functionPointer.ReturnTypeInfo);
                foreach (var parameter in functionPointer.Parameters)
                {
                    ClearLocationForTypeInfo(parameter.TypeInfo);
                }

                _functionPointers.Add(functionPointer);
                break;
            case CMacroObject macroObject:
                ClearLocationForTypeInfo(macroObject.TypeInfo);
                _macroObjects.Add(macroObject);
                break;
            case CTypeAlias typeAlias:
                ClearLocationForTypeInfo(typeAlias.UnderlyingTypeInfo);
                _typeAliases.Add(typeAlias);
                break;
            case CEnumConstant enumConstant:
                ClearLocationForTypeInfo(enumConstant.TypeInfo);
                _enumConstants.Add(enumConstant);
                break;
            default:
                throw new NotImplementedException($"Unknown node type '{node.GetType()}'");
        }
    }

    private void ClearLocationForTypeInfo(CTypeInfo typeInfo)
    {
        var currentTypeInfo = typeInfo;
        while (currentTypeInfo != null)
        {
            currentTypeInfo.Location = null;
            currentTypeInfo = currentTypeInfo.InnerTypeInfo;
        }
    }

    private void BuildCrossPlatformNodes(
        ImmutableArray<TargetPlatform> platforms,
        ImmutableArray<CNodeWithTargetPlatform> nodes,
        string nodeName)
    {
        if (nodes.Length != platforms.Length)
        {
            var nodePlatforms = nodes.Select(x => x.TargetPlatform);
            var missingNodePlatforms = platforms.Except(nodePlatforms);
            var missingNodePlatformsString = string.Join(", ", missingNodePlatforms);
            LogNodeNotCrossPlatform(nodeName, missingNodePlatformsString);
            return;
        }

        if (nodes.Length == 1)
        {
            AddCrossPlatformNode(nodes[0]);
            return;
        }

        var areAllEqual = true;
        var firstNode = nodes[0].Node;
        for (var i = 1; i < nodes.Length; i++)
        {
            var node = nodes[i].Node;

            if (!node.Equals(firstNode))
            {
                if (node is CMacroObject nodeMacroObject && firstNode is CMacroObject firstNodeMacroObject)
                {
                    if (nodeMacroObject.EqualsWithoutValue(firstNodeMacroObject))
                    {
                        areAllEqual = false;
                        break;
                    }
                }

                LogNodeNotEqual(nodeName);
                areAllEqual = false;
                break;
            }
        }

        if (areAllEqual)
        {
            AddCrossPlatformNode(nodes[0]);
        }
    }

    private ImmutableSortedDictionary<string, ImmutableArray<CNodeWithTargetPlatform>> GetPlatformNodesByName(
        ImmutableArray<CFfiTargetPlatform> platformFfis)
    {
        var platformNodesByName = new Dictionary<string, List<CNodeWithTargetPlatform>>();

        foreach (var ffi in platformFfis)
        {
            AddPlatformFfi(ffi, platformNodesByName);
        }

        var result = platformNodesByName.
            ToImmutableSortedDictionary(
                x => x.Key,
                y => y.Value.ToImmutableArray());
        return result;
    }

    private void AddPlatformFfi(
        CFfiTargetPlatform ffi,
        Dictionary<string, List<CNodeWithTargetPlatform>> platformNodesByName)
    {
        foreach (var (name, node) in ffi.Enums)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.Functions)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.Records)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.Variables)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.EnumConstants)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.FunctionPointers)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.MacroObjects)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.OpaqueTypes)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }

        foreach (var (name, node) in ffi.TypeAliases)
        {
            AddPlatformNode(name, node, ffi.PlatformRequested, platformNodesByName);
        }
    }

    private void AddPlatformNode(
        string name,
        CNode node,
        TargetPlatform targetPlatform,
        Dictionary<string, List<CNodeWithTargetPlatform>> platformNodesByName)
    {
        if (!platformNodesByName.TryGetValue(name, out var platformNodes))
        {
            platformNodes = new List<CNodeWithTargetPlatform>();
            platformNodesByName.Add(name, platformNodes);
        }

        if (platformNodes.Count >= 1)
        {
            var previousNode = platformNodes[^1].Node;
            if (node.NodeKind != previousNode.NodeKind)
            {
                var nodeActualKind = node.NodeKind.ToString();
                var nodePlatform = targetPlatform.ToString();
                var nodeExpectedKind = previousNode.NodeKind.ToString();
                var nodePlatformExpectedKind = platformNodes[^1].TargetPlatform.ToString();
                LogNodeNotSameKind(name, nodeActualKind, nodePlatform, nodeExpectedKind, nodePlatformExpectedKind);
                return;
            }
        }

        var nodeWithTargetPlatform = new CNodeWithTargetPlatform(node, targetPlatform);
        platformNodes.Add(nodeWithTargetPlatform);
    }

    private ImmutableArray<CFfiTargetPlatform> GetPlatformFfis(ImmutableArray<string> inputFilePaths)
    {
        var builder = ImmutableArray.CreateBuilder<CFfiTargetPlatform>();

        foreach (var filePath in inputFilePaths)
        {
            CFfiTargetPlatform ffi;
            try
            {
                ffi = Json.ReadFfiTargetPlatform(_fileSystem, filePath);
            }
#pragma warning disable CA1031
            catch (Exception e)
#pragma warning restore CA1031
            {
                LogFailedToLoadTargetPlatformAbstractSyntaxTree(e, filePath);
                continue;
            }

            if (ffi.PlatformRequested.Equals(ffi.PlatformActual) && ffi.PlatformRequested.Equals(TargetPlatform.Unknown))
            {
                // Skip any FFI which is unknown platform.
                // This can happen if the cross-platform FFI is placed in the same folder as the target-platform FFIs.
                continue;
            }

            builder.Add(ffi);
        }

        return builder.ToImmutable();
    }

    [LoggerMessage(0, LogLevel.Warning, "The node '{NodeName}' is not cross-platform; there is no matching node for platforms: {MissingPlatformNames}")]
    private partial void LogNodeNotCrossPlatform(string nodeName, string missingPlatformNames);

    [LoggerMessage(1, LogLevel.Error, "The node '{NodeName}' of kind '{NodeActualKind}' for platform '{NodePlatform}' does not match the kind '{nodeExpectedKind}' for platform {NodePlatformExpectedKind}.")]
    private partial void LogNodeNotSameKind(string nodeName, string nodeActualKind, string nodePlatform, string nodeExpectedKind, string nodePlatformExpectedKind);

    [LoggerMessage(2, LogLevel.Error, "The node '{NodeName}' is not equal to all other platform nodes of the same name.")]
    private partial void LogNodeNotEqual(string nodeName);

    [LoggerMessage(3, LogLevel.Information, "Success. Merged FFIs for the target platforms '{TargetPlatformsString}': {FilePath}")]
    private partial void LogWriteAbstractSyntaxTreeSuccess(
        string targetPlatformsString,
        string filePath);

    [LoggerMessage(4, LogLevel.Error, "Failed to load platform FFI: {FilePath}")]
    private partial void LogFailedToLoadTargetPlatformAbstractSyntaxTree(Exception e, string filePath);
}
