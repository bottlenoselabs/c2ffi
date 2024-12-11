// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace c2ffi.Extract.Parse;

internal sealed class ClangDiagnostic
{
    public bool IsErrorOrFatal { get; set; }

    public string Message { get; set; } = string.Empty;
}
