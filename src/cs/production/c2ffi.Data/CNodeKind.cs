// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;

namespace c2ffi.Data;

/// <summary>
///     Defines the kind of C nodes in the <see cref="CFfiTargetPlatform" />.
/// </summary>
[PublicAPI]
public enum CNodeKind
{
    /// <summary>
    ///     An unspecified or unrecognized C node.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     A primitive data type in C.
    /// </summary>
    Primitive,

    /// <summary>
    ///     A pointer in C.
    /// </summary>
    Pointer,

    /// <summary>
    ///     An array in C.
    /// </summary>
    Array,

    /// <summary>
    ///     A function in C.
    /// </summary>
    Function,

    /// <summary>
    ///     A function parameter in C.
    /// </summary>
    FunctionParameter,

    /// <summary>
    ///     A function pointer in C.
    /// </summary>
    FunctionPointer,

    /// <summary>
    ///     A parameter for a function pointer in C.
    /// </summary>
    FunctionPointerParameter,

    /// <summary>
    ///     A struct in C.
    /// </summary>
    Struct,

    /// <summary>
    ///     A union in C.
    /// </summary>
    Union,

    /// <summary>
    ///     A field in a struct or union in C.
    /// </summary>
    RecordField,

    /// <summary>
    ///     An enumeration in C.
    /// </summary>
    Enum,

    /// <summary>
    ///     A value of an enumeration in C.
    /// </summary>
    EnumValue,

    /// <summary>
    ///     An opaque type in C.
    /// </summary>
    OpaqueType,

    /// <summary>
    ///     A type alias in C.
    /// </summary>
    TypeAlias,

    /// <summary>
    ///     A variable in C.
    /// </summary>
    Variable,

    /// <summary>
    ///     An object-like macro in C.
    /// </summary>
    MacroObject
}
