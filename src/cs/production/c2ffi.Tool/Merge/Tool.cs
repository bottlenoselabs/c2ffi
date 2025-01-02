// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using bottlenoselabs.Common.Tools;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Data.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace c2ffi.Merge;

[UsedImplicitly]
public sealed partial class Tool(
    ILogger<Tool> logger,
    IFileSystem fileSystem,
    InputSanitizer inputSanitizer) : Tool<InputUnsanitized, InputSanitized, Output>(logger, inputSanitizer, fileSystem)
{
    private readonly IFileSystem _fileSystem = fileSystem;

    private readonly List<CEnum> _enums = [];
    private readonly List<CVariable> _variables = [];
    private readonly List<COpaqueType> _opaqueTypes = [];
    private readonly List<CFunction> _functions = [];
    private readonly List<CRecord> _records = [];
    private readonly List<CFunctionPointer> _functionPointers = [];
    private readonly List<CMacroObject> _macroObjects = [];
    private readonly List<CTypeAlias> _typeAliases = [];

    private sealed class CNodeWithTargetPlatform(CNode node, TargetPlatform targetPlatform)
    {
        public readonly CNode Node = node;
        public readonly TargetPlatform TargetPlatform = targetPlatform;
    }

    public void Run(string inputDirectoryPath, string outputFilePath)
    {
        var unsanitizedInput = new InputUnsanitized
        {
            InputDirectoryPath = inputDirectoryPath,
            OutputFilePath = outputFilePath
        };

        _ = Run(unsanitizedInput);
    }

    protected override void Execute(InputSanitized inputSanitized, Output output)
    {
        var platformFfis =
            GetPlatformFfis(inputSanitized.InputFilePaths);
        var platforms = platformFfis.
            Select(x => x.PlatformRequested).ToImmutableArray();
        var platformNodesByKey = GetPlatformNodesByKey(platformFfis);
        var ffi = CreateCrossPlatformFfi(platforms, platformNodesByKey);

        Json.WriteFfiCrossPlatform(_fileSystem, inputSanitized.OutputFilePath, ffi);
        LogWriteAbstractSyntaxTreeSuccess(string.Join(", ", platforms), inputSanitized.OutputFilePath);
    }

    private CFfiCrossPlatform CreateCrossPlatformFfi(
        ImmutableArray<TargetPlatform> platforms,
        ImmutableSortedDictionary<string, ImmutableArray<CNodeWithTargetPlatform>> platformNodesByKey)
    {
        var result = new CFfiCrossPlatform();

        foreach (var (_, nodes) in platformNodesByKey)
        {
            BuildCrossPlatformNodes(platforms, nodes);
        }

        result.Platforms = platforms.Sort(
            (a, b) =>
                string.Compare(a.ClangTargetTriple, b.ClangTargetTriple, StringComparison.Ordinal));
        result.Enums = _enums.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.Variables = _variables.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.OpaqueTypes = _opaqueTypes.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.Functions = _functions.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.Records = _records.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.FunctionPointers = _functionPointers.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.MacroObjects = _macroObjects.ToImmutableSortedDictionary(x => x.Name, x => x);
        result.TypeAliases = _typeAliases.ToImmutableSortedDictionary(x => x.Name, x => x);
        return result;
    }

    private void AddCrossPlatformNode(CNodeWithTargetPlatform nodeWithTargetPlatform)
    {
        var node = nodeWithTargetPlatform.Node;

        if (node is CNodeWithLocation nodeWithLocation)
        {
            var location = nodeWithLocation.Location!.Value;
            // Strip the file path as it's usually platform specific
            nodeWithLocation.Location = new CLocation
            {
                FileName = location.FileName,
                FilePath = string.Empty,
                LineColumn = location.LineColumn,
                LineNumber = location.LineNumber,
                IsSystem = location.IsSystem
            };
        }

        switch (node)
        {
            case CEnum @enum:
                _enums.Add(@enum);
                break;
            case CVariable variable:
                _variables.Add(variable);
                break;
            case COpaqueType opaqueType:
                _opaqueTypes.Add(opaqueType);
                break;
            case CFunction function:
                ClearLocationForTypeInfo(function.ReturnType);
                foreach (var parameter in function.Parameters)
                {
                    parameter.Location = null;
                    ClearLocationForTypeInfo(parameter.Type);
                }

                _functions.Add(function);
                break;
            case CRecord record:
                foreach (var field in record.Fields)
                {
                    field.Location = null;
                    ClearLocationForTypeInfo(field.Type);
                }

                _records.Add(record);
                break;
            case CFunctionPointer functionPointer:
                ClearLocationForTypeInfo(functionPointer.ReturnType);
                foreach (var parameter in functionPointer.Parameters)
                {
                    ClearLocationForTypeInfo(parameter.Type);
                }

                _functionPointers.Add(functionPointer);
                break;
            case CMacroObject macroObject:
                ClearLocationForTypeInfo(macroObject.Type);
                _macroObjects.Add(macroObject);
                break;
            case CTypeAlias typeAlias:
                ClearLocationForTypeInfo(typeAlias.UnderlyingType);
                _typeAliases.Add(typeAlias);
                break;
            default:
                throw new NotImplementedException($"Unknown node type '{node.GetType()}'");
        }
    }

    private void ClearLocationForTypeInfo(CType type)
    {
        var currentTypeInfo = type;
        while (currentTypeInfo != null)
        {
            currentTypeInfo.Location = null;
            currentTypeInfo = currentTypeInfo.InnerType;
        }
    }

    private void BuildCrossPlatformNodes(
        ImmutableArray<TargetPlatform> platforms,
        ImmutableArray<CNodeWithTargetPlatform> nodes)
    {
        var nodeName = nodes.FirstOrDefault()!.Node.Name;
        var targetPlatform = nodes.FirstOrDefault()!.TargetPlatform.ToString();

        if (nodes.Length != platforms.Length)
        {
            var nodePlatforms = nodes.Select(x => x.TargetPlatform);
            var missingNodePlatforms = platforms.Except(nodePlatforms);
            var missingNodePlatformsString = string.Join(", ", missingNodePlatforms);
            LogNodeNotCrossPlatform(nodeName, targetPlatform, missingNodePlatformsString);
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

            var nodesAreEqual = node.Equals(firstNode);
            if (!nodesAreEqual)
            {
                var x = node.Equals(firstNode);

                if (node is CMacroObject nodeMacroObject && firstNode is CMacroObject firstNodeMacroObject)
                {
                    if (nodeMacroObject.EqualsWithoutValue(firstNodeMacroObject))
                    {
                        areAllEqual = false;
                        break;
                    }
                }

                LogNodeNotEqual(nodeName, firstNode.NodeKind.ToString());
                areAllEqual = false;
                break;
            }
        }

        if (areAllEqual)
        {
            AddCrossPlatformNode(nodes[0]);
        }
    }

    private ImmutableSortedDictionary<string, ImmutableArray<CNodeWithTargetPlatform>> GetPlatformNodesByKey(
        ImmutableArray<CFfiTargetPlatform> platformFfis)
    {
        var platformNodesByKey = new Dictionary<string, List<CNodeWithTargetPlatform>>();

        foreach (var ffi in platformFfis)
        {
            AddPlatformFfi(ffi, platformNodesByKey);
        }

        var result = platformNodesByKey.
            ToImmutableSortedDictionary(
                x => x.Key,
                y => y.Value.ToImmutableArray());
        return result;
    }

    private void AddPlatformFfi(
        CFfiTargetPlatform ffi,
        Dictionary<string, List<CNodeWithTargetPlatform>> platformNodesByKey)
    {
        var nodes = ImmutableArray.CreateBuilder<CNode>();
        nodes.AddRange(ffi.Enums.Values);
        nodes.AddRange(ffi.Functions.Values);
        nodes.AddRange(ffi.Records.Values);
        nodes.AddRange(ffi.Variables.Values);
        nodes.AddRange(ffi.FunctionPointers.Values);
        nodes.AddRange(ffi.MacroObjects.Values);
        nodes.AddRange(ffi.OpaqueTypes.Values);
        nodes.AddRange(ffi.TypeAliases.Values);

        foreach (var node in nodes.ToImmutable())
        {
            AddPlatformNode(node, ffi.PlatformRequested, platformNodesByKey);
        }
    }

    private void AddPlatformNode(
        CNode node,
        TargetPlatform targetPlatform,
        Dictionary<string, List<CNodeWithTargetPlatform>> platformNodesByKey)
    {
        var key = $"{node.NodeKind}:{node.Name}";

        if (!platformNodesByKey.TryGetValue(key, out var platformNodes))
        {
            platformNodes = [];
            platformNodesByKey.Add(key, platformNodes);
        }

        if (platformNodes.Count >= 1)
        {
            var previousNode = platformNodes[^1].Node;
            if (node.NodeKind != previousNode.NodeKind)
            {
                var nodeKindString = node.NodeKind.ToString();
                var targetPlatformString = targetPlatform.ToString();
                var nodeKindExpected = previousNode.NodeKind.ToString();
                var platformNameExpected = platformNodes[^1].TargetPlatform.ToString();
                LogNodeNotSameKind(
                    node.Name,
                    nodeKindString,
                    targetPlatformString,
                    nodeKindExpected,
                    platformNameExpected);
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

    [LoggerMessage(0, LogLevel.Warning, "The node '{NodeName}' for platform '{PlatformName}' is not cross-platform; there is no matching node for platforms: {MissingPlatformNames}")]
    private partial void LogNodeNotCrossPlatform(string nodeName, string platformName, string missingPlatformNames);

    [LoggerMessage(1, LogLevel.Error, "The node '{NodeName}' of kind '{NodeKind}' for platform '{PlatformName}' does not match the kind '{NodeKindExpected}' for platform {PlatformNameExpected}.")]
    private partial void LogNodeNotSameKind(string nodeName, string nodeKind, string platformName, string nodeKindExpected, string platformNameExpected);

    [LoggerMessage(2, LogLevel.Error, "The node '{NodeName}' of kind '{NodeKind}' is not equal to all other platform nodes of the same name.")]
    private partial void LogNodeNotEqual(string nodeName, string nodeKind);

    [LoggerMessage(3, LogLevel.Information, "Success. Merged FFIs for the target platforms '{TargetPlatformsString}': {FilePath}")]
    private partial void LogWriteAbstractSyntaxTreeSuccess(
        string targetPlatformsString,
        string filePath);

    [LoggerMessage(4, LogLevel.Error, "Failed to load platform FFI: {FilePath}")]
    private partial void LogFailedToLoadTargetPlatformAbstractSyntaxTree(Exception e, string filePath);
}
