// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text;
using bottlenoselabs;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using c2ffi.Tool.Commands.Extract.Input.Sanitized;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Parse;

public sealed partial class ClangTranslationUnitParser
{
    private readonly ILogger<ClangTranslationUnitParser> _logger;
    private readonly ParseArgumentsProvider _argumentsProvider;
    private readonly ParseSystemIncludeDirectoriesProvider _systemIncludeDirectoriesProvider;

    public ClangTranslationUnitParser(
        ILogger<ClangTranslationUnitParser> logger,
        ParseArgumentsProvider argumentsProvider,
        ParseSystemIncludeDirectoriesProvider systemIncludeDirectoriesProvider)
    {
        _logger = logger;
        _argumentsProvider = argumentsProvider;
        _systemIncludeDirectoriesProvider = systemIncludeDirectoriesProvider;
    }

    public ParseContext ParseTranslationUnit(
        string filePath,
        ExtractTargetPlatformOptions extractOptions,
        bool isCPlusPlus = false,
        bool ignoreWarnings = false,
        bool logClangDiagnostics = false,
        bool keepGoing = false)
    {
        var systemIncludeDirectories = _systemIncludeDirectoriesProvider.GetSystemIncludeDirectories(
            extractOptions.TargetPlatform, extractOptions.UserIncludeDirectories, extractOptions.IsEnabledFindSystemHeaders);
        var arguments = _argumentsProvider.GetArguments(
            extractOptions, systemIncludeDirectories, isCPlusPlus, ignoreWarnings);
        var argumentsString = string.Join(" ", arguments);

        if (!ClangExtensions.TryParseTranslationUnit(filePath, arguments, out var translationUnit, true, keepGoing))
        {
            var up = new ClangException($"Failed to parse the file as translation unit: {filePath}");
            LogFailureInvalidArguments(filePath, argumentsString, up);
            throw up;
        }

        if (logClangDiagnostics)
        {
            LogClangDiagnostics(filePath, translationUnit, argumentsString);
        }

        var result = new ParseContext(
            translationUnit,
            filePath,
            extractOptions,
            arguments,
            systemIncludeDirectories);
        return result;
    }

    private void LogClangDiagnostics(
        string filePath,
        clang.CXTranslationUnit translationUnit,
        string argumentsString)
    {
        var isSuccess = true;

        var clangDiagnostics = GetClangDiagnostics(translationUnit);
        var stringBuilder = new StringBuilder();

        if (!clangDiagnostics.IsDefaultOrEmpty)
        {
            foreach (var clangDiagnostic in clangDiagnostics)
            {
                if (clangDiagnostic.IsErrorOrFatal)
                {
                    isSuccess = false;
                }

                stringBuilder.AppendLine(clangDiagnostic.Message);
            }
        }

        var clangDiagnosticMessagesJoined = stringBuilder.ToString();

        if (isSuccess)
        {
            LogSuccessWithDiagnostics(filePath, argumentsString, clangDiagnosticMessagesJoined);
        }
        else
        {
            LogFailureWithDiagnostics(filePath, argumentsString, clangDiagnosticMessagesJoined);
        }
    }

    private static ImmutableArray<ClangDiagnostic> GetClangDiagnostics(CXTranslationUnit translationUnit)
    {
        var diagnosticsCount = (int)clang_getNumDiagnostics(translationUnit);
        var builder = ImmutableArray.CreateBuilder<ClangDiagnostic>(diagnosticsCount);

        var defaultDisplayOptions = clang_defaultDiagnosticDisplayOptions();
        for (uint i = 0; i < diagnosticsCount; ++i)
        {
            var diagnostic = GetClangDiagnostic(translationUnit, i, defaultDisplayOptions);
            builder.Add(diagnostic);
        }

        return builder.ToImmutable();
    }

    private static ClangDiagnostic GetClangDiagnostic(
        CXTranslationUnit translationUnit,
        uint index,
        uint defaultDisplayOptions)
    {
        var clangDiagnostic = clang_getDiagnostic(translationUnit, index);
        var clangString = clang_formatDiagnostic(clangDiagnostic, defaultDisplayOptions);
        var diagnosticString = clangString.String();
        var severity = clang_getDiagnosticSeverity(clangDiagnostic);
        var isErrorOrFatal = severity is
            CXDiagnosticSeverity.CXDiagnostic_Error or
            CXDiagnosticSeverity.CXDiagnostic_Fatal;

        var diagnostic = new ClangDiagnostic
        {
            IsErrorOrFatal = isErrorOrFatal,
            Message = diagnosticString
        };
        return diagnostic;
    }

    [LoggerMessage(
        0,
        LogLevel.Error,
        "- Failed. The arguments are incorrect or invalid. Path: {FilePath} ; Clang arguments: {Arguments}")]
    private partial void LogFailureInvalidArguments(string filePath, string arguments, Exception exception);

    [LoggerMessage(
        1,
        LogLevel.Debug,
        "- Success. Path: {FilePath} ; Clang arguments: {Arguments} ; Diagnostics: {DiagnosticMessagesJoined}")]
    private partial void LogSuccessWithDiagnostics(string filePath, string arguments, string diagnosticMessagesJoined);

    [LoggerMessage(
        2,
        LogLevel.Error,
        "- Failed. One or more Clang diagnostics are reported when parsing that are an error or fatal. Path: {FilePath} ; Clang arguments: {Arguments} ; Diagnostics: {DiagnosticMessagesJoined}")]
    private partial void LogFailureWithDiagnostics(string filePath, string arguments, string diagnosticMessagesJoined);
}
