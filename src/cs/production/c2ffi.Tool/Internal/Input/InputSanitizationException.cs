// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace c2ffi.Tool.Internal.Input;

public sealed class InputSanitizationException : Exception
{
    public InputSanitizationException()
    {
    }

    public InputSanitizationException(string message)
        : base(message)
    {
    }

    public InputSanitizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
