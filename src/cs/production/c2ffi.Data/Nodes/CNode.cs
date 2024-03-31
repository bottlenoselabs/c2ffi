// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data.Nodes;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents the abstract base for a node in a C foreign function interface.
/// </summary>
[PublicAPI]
public abstract class CNode : IComparable<CNode>, IEquatable<CNode>
{
    /// <summary>
    ///     Gets or sets the comment associated with the C node.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the C node originates from a system header.
    /// </summary>
    [JsonPropertyName("is_system")]
    public bool IsSystem { get; set; }

    /// <summary>
    ///     Gets or sets the name of the C node.
    /// </summary>
    [JsonIgnore]
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    internal CNodeKind NodeKind => GetKind(this);

    /// <summary>
    ///     Compares the name of this <see cref="CNode" /> instance with another <see cref="CNode" /> instance.
    /// </summary>
    /// <param name="other">The other <see cref="CNode" />.</param>
    /// <returns>A <see cref="int" />.</returns>
    public int CompareTo(CNode? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        var result = CompareToInternal(other);
        return result;
    }

    /// <summary>
    ///     Overridable. Compares the name of this <see cref="CNode" /> instance with another <see cref="CNode" />
    ///     instance.
    /// </summary>
    /// <param name="other">The other <see cref="CNode" />.</param>
    /// <returns>A <see cref="int" />.</returns>
    protected virtual int CompareToInternal(CNode? other)
    {
        if (other == null)
        {
            return 0;
        }

        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }

    private CNodeKind GetKind(CNode node)
    {
        return this switch
        {
            CEnum => CNodeKind.Enum,
            CEnumValue => CNodeKind.EnumValue,
            CFunction => CNodeKind.Function,
            CFunctionParameter => CNodeKind.FunctionParameter,
            CFunctionPointer => CNodeKind.FunctionPointer,
            CFunctionPointerParameter => CNodeKind.FunctionPointerParameter,
            COpaqueType => CNodeKind.OpaqueType,
            CRecord => ((CRecord)node).RecordKind == CRecordKind.Struct ? CNodeKind.Struct : CNodeKind.Union,
            CTypeAlias => CNodeKind.TypeAlias,
            CVariable => CNodeKind.Variable,
            CMacroObject => CNodeKind.MacroObject,
            CRecordField => CNodeKind.RecordField,
            CPrimitive => CNodeKind.Primitive,
            CPointer => CNodeKind.Pointer,
            CArray => CNodeKind.Array,
            _ => throw new NotImplementedException($"The mapping of the kind for '{GetType()}' is not implemented.")
        };
    }

    public static bool operator <(CNode first, CNode second)
    {
        return first.CompareTo(second) < 0;
    }

    public static bool operator >(CNode first, CNode second)
    {
        return first.CompareTo(second) > 0;
    }

    public static bool operator >=(CNode first, CNode second)
    {
        return first.CompareTo(second) >= 0;
    }

    public static bool operator <=(CNode first, CNode second)
    {
        return first.CompareTo(second) <= 0;
    }

    public virtual bool Equals(CNode? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CNode)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(Name, Comment);
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    public static bool operator ==(CNode? left, CNode? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(CNode? left, CNode? right)
    {
        return !(left == right);
    }
}
