// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;

namespace c2ffi.Data;

/// <summary>
///     Defines the types of calling conventions in C.
/// </summary>
[PublicAPI]
public enum CFunctionCallingConvention
{
    /// <summary>
    ///     An unknown calling convention.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     The 'cdecl' calling convention, where function arguments are pushed onto the stack in a right-to-left order,
    ///     and the caller is responsible for cleaning up the stack after the function returns.
    /// </summary>
    // ReSharper disable once IdentifierTypo
    Cdecl,

    /// <summary>
    ///     The 'stdcall' calling convention, where function arguments are pushed onto the stack in a right-to-left
    ///     order, and the callee is responsible for cleaning up the stack before returning.
    /// </summary>
    StdCall,

    /// <summary>
    ///     The 'fastcall' calling convention, where the first few arguments are typically passed in registers, and the
    ///     rest are pushed onto the stack.
    /// </summary>
    FastCall
}
