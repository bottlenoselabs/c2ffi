// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Clang;
using c2ffi.Data;
using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

#pragma warning disable IDE0130
namespace c2ffi.Extract.Explore;
#pragma warning restore IDE0130

[UsedImplicitly]
internal sealed class FunctionExplorer(ILogger<FunctionExplorer> logger)
    : NodeExplorer<CFunction>(logger, false)
{
    // NOTE: Function visiting by name.
    //  When a header file contains the declaration of the function, it may later be implemented in the same header
    //  file or another header file. When this happens there will be two function declaration cursors with the same
    //  name even if they have the same type signature (result type and parameter types).
    //  For this reason, do not log if already visited.

    protected override KindCursors ExpectedCursors { get; } =
        KindCursors.Is(CXCursorKind.CXCursor_FunctionDecl);

    protected override KindTypes ExpectedTypes { get; } = KindTypes.Either(
        CXTypeKind.CXType_FunctionProto, CXTypeKind.CXType_FunctionNoProto);

    protected override bool IsAllowed(ExploreContext exploreContext, NodeInfo info)
    {
        var regexes = exploreContext.ParseContext.InputSanitized.IgnoredFunctionRegexes;
        foreach (var regex in regexes)
        {
            if (regex.IsMatch(info.Name))
            {
                return false;
            }
        }

        return true;
    }

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        var function = Function(exploreContext, info);
        return function;
    }

    private CFunction Function(ExploreContext exploreContext, NodeInfo info)
    {
        var returnType = FunctionReturnType(exploreContext, info);
        var parameters = FunctionParameters(exploreContext, info);
        var callingConvention = FunctionCallingConvention(info.ClangType);
        var comment = exploreContext.Comment(info.ClangCursor);

        var result = new CFunction
        {
            Name = info.Name,
            Location = info.Location,
            ReturnType = returnType,
            Parameters = parameters,
            CallingConvention = callingConvention,
            Comment = comment
        };

        return result;
    }

    private static CFunctionCallingConvention FunctionCallingConvention(CXType type)
    {
        var callingConvention = clang_getFunctionTypeCallingConv(type);
#pragma warning disable IDE0072
        var result = callingConvention switch
#pragma warning restore IDE0072
        {
            CXCallingConv.CXCallingConv_C => CFunctionCallingConvention.Cdecl,
            CXCallingConv.CXCallingConv_X86StdCall => CFunctionCallingConvention.StdCall,
            CXCallingConv.CXCallingConv_X86FastCall => CFunctionCallingConvention.FastCall,
            _ => CFunctionCallingConvention.Unknown
        };

        return result;
    }

    private static CType FunctionReturnType(
        ExploreContext exploreContext, NodeInfo parentInfo)
    {
        var resultType = clang_getCursorResultType(parentInfo.ClangCursor);
        return exploreContext.VisitType(resultType, parentInfo);
    }

    private ImmutableArray<CFunctionParameter> FunctionParameters(
        ExploreContext exploreContext,
        NodeInfo info)
    {
        var builder = ImmutableArray.CreateBuilder<CFunctionParameter>();

        var count = clang_Cursor_getNumArguments(info.ClangCursor);
        for (uint i = 0; i < count; i++)
        {
            var parameterCursor = clang_Cursor_getArgument(info.ClangCursor, i);
            var functionParameter = FunctionParameter(exploreContext, parameterCursor, info);

            builder.Add(functionParameter);
        }

        var result = builder.ToImmutable();
        return result;
    }

    private static CFunctionParameter FunctionParameter(
        ExploreContext exploreContext,
        CXCursor parameterCursor,
        NodeInfo parentInfo)
    {
        var name = parameterCursor.Spelling();
        var parameterType = clang_getCursorType(parameterCursor);

        var parameterTypeInfo = exploreContext.VisitType(parameterType, parentInfo);
        var comment = exploreContext.Comment(parameterCursor);

        var functionExternParameter = new CFunctionParameter
        {
            Name = name,
            Location = parameterTypeInfo.Location,
            Type = parameterTypeInfo,
            Comment = comment
        };
        return functionExternParameter;
    }
}
