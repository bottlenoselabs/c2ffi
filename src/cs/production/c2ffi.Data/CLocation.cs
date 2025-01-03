// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace c2ffi.Data;

// NOTE: Properties are required for System.Text.Json serialization

/// <summary>
///     Represents a file location in a C header file.
/// </summary>
[PublicAPI]
public record struct CLocation : IComparable<CLocation>
{
    /// <summary>
    ///     Gets or sets the file name of the C header file with it's file extension but without it's directory path.
    /// </summary>
    [JsonPropertyName("file_name")]
    public string FileName { get; set; }

    /// <summary>
    ///     Gets or sets the relative file path of the C header file.
    /// </summary>
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; }

    /// <summary>
    ///     Gets or sets the full file path of the C header file.
    /// </summary>
    [JsonIgnore]
    public string FullFilePath { get; set; }

    /// <summary>
    ///     Gets or sets the line number in the C header file.
    /// </summary>
    [JsonPropertyName("line")]
    public int LineNumber { get; set; }

    /// <summary>
    ///     Gets or sets the column number in the C header file.
    /// </summary>
    [JsonPropertyName("column")]
    public int LineColumn { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the location originates from a system header.
    /// </summary>
    [JsonPropertyName("is_system")]
    public bool IsSystem { get; set; }

    /// <summary>
    ///     Compares the file appearance of this <see cref="CLocation" /> instance with another <see cref="CLocation" />
    ///     instance.
    /// </summary>
    /// <param name="other">The other <see cref="CLocation" />.</param>
    /// <returns>A <see cref="int" />.</returns>
    public readonly int CompareTo(CLocation other)
    {
        var result = string.Compare(FileName, other.FileName, StringComparison.Ordinal);
        if (result != 0)
        {
            return result;
        }

        result = LineNumber.CompareTo(other.LineNumber);
        if (result != 0)
        {
            return result;
        }

        result = LineColumn.CompareTo(other.LineColumn);
        return result;
    }

    /// <inheritdoc />
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        if (LineNumber == 0 && LineColumn == 0)
        {
            return $"{FileName}";
        }

        return string.IsNullOrEmpty(FilePath) || FilePath == FileName
            ? $"{FileName}:{LineNumber}:{LineColumn}"
            : $"{FileName}:{LineNumber}:{LineColumn} ({FilePath})";
    }

    public static bool operator <(CLocation first, CLocation second)
    {
        return first.CompareTo(second) < 0;
    }

    public static bool operator >(CLocation first, CLocation second)
    {
        return first.CompareTo(second) > 0;
    }

    public static bool operator >=(CLocation first, CLocation second)
    {
        return first.CompareTo(second) >= 0;
    }

    public static bool operator <=(CLocation first, CLocation second)
    {
        return first.CompareTo(second) <= 0;
    }
}
