// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Domain.Explore.Context;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class FunctionPointerExplorer(ILogger<FunctionPointerExplorer> logger)
    : NodeExplorer<CFunctionPointer>(logger, false)
{
    // NOTE: Function pointer visiting by name.
    //  (1) A typedef can be an alias to a function pointer. Typedefs are declarations, declarations always have a
    //      cursor. The name of the function pointer is the name of the typedef. Thus, there can only ever be one
    //      function pointer with that name.
    //  (2) A function pointer can be inlined either to a: struct field, function result, function parameter,
    //      function pointer result, function pointer parameter. Take a moment to think about the last two ones; yes,
    //      an inlined function pointer can be nested inside another function pointer's result or parameters.
    //      This can happen recursively. The name of an inlined function pointer is taken from it's type signature.
    //      This means that it is possible for a function pointer by type to visited multiple times from different
    //      locations (struct field, function result, function parameter, function pointer result, function pointer
    //      parameter).

    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Either(
        CXTypeKind.CXType_FunctionProto, CXTypeKind.CXType_FunctionNoProto);

    protected override CNode GetNode(ExploreContext context, ExploreNodeInfo info)
    {
        return FunctionPointer(context, info);
    }

    private static CFunctionPointer FunctionPointer(ExploreContext context, ExploreNodeInfo info)
    {
        var type = context.VisitType(info.ClangType, info, nodeKind: CNodeKind.FunctionPointer);
        var returnType = FunctionPointerReturnType(context, info);
        var parameters = FunctionPointerParameters(context, info);
        var callingConvention = FunctionPointerCallingConvention(info.ClangType);
        var comment = context.Comment(info.ClangCursor);

        var result = new CFunctionPointer
        {
            Name = info.Name,
            Location = info.Location,
            CallingConvention = callingConvention,
            Type = type,
            ReturnType = returnType,
            Parameters = parameters,
            Comment = comment
        };

        return result;
    }

    private static CType FunctionPointerReturnType(ExploreContext context, ExploreNodeInfo info)
    {
        var returnType = clang_getResultType(info.ClangType);
        var returnTypeInfo = context.VisitType(returnType, info)!;
        return returnTypeInfo;
    }

    private static ImmutableArray<CFunctionPointerParameter> FunctionPointerParameters(
        ExploreContext context,
        ExploreNodeInfo info)
    {
        var builder = ImmutableArray.CreateBuilder<CFunctionPointerParameter>();

        var count = clang_getNumArgTypes(info.ClangType);
        for (uint i = 0; i < count; i++)
        {
            var parameterType = clang_getArgType(info.ClangType, i);
            var functionPointerParameter = FunctionPointerParameter(context, parameterType, info);
            builder.Add(functionPointerParameter);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private static CFunctionPointerParameter FunctionPointerParameter(
        ExploreContext context,
        CXType parameterType,
        ExploreNodeInfo parentInfo)
    {
        var parameterTypeInfo = context.VisitType(parameterType, parentInfo)!;

        var result = new CFunctionPointerParameter
        {
            Name = string.Empty,
            Type = parameterTypeInfo
        };
        return result;
    }

    private static CFunctionCallingConvention FunctionPointerCallingConvention(CXType type)
    {
        var callingConvention = clang_getFunctionTypeCallingConv(type);
        var result = callingConvention switch
        {
            CXCallingConv.CXCallingConv_C => CFunctionCallingConvention.Cdecl,
            CXCallingConv.CXCallingConv_X86StdCall => CFunctionCallingConvention.StdCall,
            CXCallingConv.CXCallingConv_X86FastCall => CFunctionCallingConvention.FastCall,
            _ => CFunctionCallingConvention.Unknown
        };

        return result;
    }
}
