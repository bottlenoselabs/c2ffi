// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;

namespace c2ffi.Data;

/// <summary>
///     Defines the kind of C records.
/// </summary>
[PublicAPI]
public enum CRecordKind
{
    /// <summary>
    ///     The record represents a structure.
    /// </summary>
    Struct,

    /// <summary>
    ///     The record represents a union.
    /// </summary>
    Union
}
