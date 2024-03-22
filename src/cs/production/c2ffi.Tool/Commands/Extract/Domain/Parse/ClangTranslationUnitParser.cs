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
    private readonly ParseContextBuilderArgumentsProvider _argumentsProvider;
    private readonly ParseContextBuilderSystemIncludeDirectoriesProvider _systemIncludeDirectoriesProvider;

    public ClangTranslationUnitParser(
        ILogger<ClangTranslationUnitParser> logger,
        ParseContextBuilderArgumentsProvider argumentsProvider,
        ParseContextBuilderSystemIncludeDirectoriesProvider systemIncludeDirectoriesProvider)
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

        if (!TryParseTranslationUnit(filePath, arguments, out var translationUnit, true, keepGoing))
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

    private static unsafe bool TryParseTranslationUnit(
        string filePath,
        ImmutableArray<string> commandLineArgs,
        out CXTranslationUnit translationUnit,
        bool skipFunctionBodies = true,
        bool keepGoing = false)
    {
        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        uint options = 0x0 |
                       0x1 | // CXTranslationUnit_DetailedPreprocessingRecord
                       0x80 | // IncludeBriefCommentsInCodeCompletion
                       0x1000 | // CXTranslationUnit_IncludeAttributedTypes
                       0x2000 | // CXTranslationUnit_VisitImplicitAttributes
                       0x4000 | // CXTranslationUnit_IgnoreNonErrorsFromIncludedFiles
                       0x0;

        if (skipFunctionBodies)
        {
            options |= 0x40; // CXTranslationUnit_SkipFunctionBodies
        }

        if (keepGoing)
        {
            options |= 0x200; // CXTranslationUnit_KeepGoing
        }

        var index = clang_createIndex(0, 0);
        var cSourceFilePath = CString.FromString(filePath);
        var cCommandLineArgs = CStrings.CStringArray(commandLineArgs.AsSpan());

        CXErrorCode errorCode;
        fixed (CXTranslationUnit* translationUnitPointer = &translationUnit)
        {
            errorCode = clang_parseTranslationUnit2(
                index,
                cSourceFilePath,
                cCommandLineArgs,
                commandLineArgs.Length,
                (CXUnsavedFile*)IntPtr.Zero,
                0,
                options,
                translationUnitPointer);
        }

        var result = errorCode == CXErrorCode.CXError_Success;
        return result;
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
