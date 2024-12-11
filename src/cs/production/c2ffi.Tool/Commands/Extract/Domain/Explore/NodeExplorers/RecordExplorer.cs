// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using c2ffi.Data.Nodes;
using c2ffi.Tool.Commands.Extract.Infrastructure.Clang;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

internal abstract class RecordExplorer(ILogger<RecordExplorer> logger, bool logAlreadyExplored = true)
    : NodeExplorer<CRecord>(logger, logAlreadyExplored)
{
    protected override ExploreKindTypes ExpectedTypes { get; } = ExploreKindTypes.Is(CXTypeKind.CXType_Record);

    protected ImmutableArray<CXCursor> FieldCursors(CXType type)
    {
        var fieldCursors = type.GetFields();
        if (fieldCursors.IsDefaultOrEmpty)
        {
            return ImmutableArray<CXCursor>.Empty;
        }

        var filteredFieldCursors = ImmutableArray.CreateBuilder<CXCursor>();
        filteredFieldCursors.Add(fieldCursors[^1]);

        for (var index = fieldCursors.Length - 2; index >= 0; index--)
        {
            var current = fieldCursors[index];
            var next = fieldCursors[index + 1];

            if (current.kind == CXCursorKind.CXCursor_UnionDecl && next.kind == CXCursorKind.CXCursor_FieldDecl)
            {
                var typeNext = clang_getCursorType(next);
                var typeCurrent = clang_getCursorType(current);

                var typeNextCursor = clang_getTypeDeclaration(typeNext);
                var typeCurrentCursor = clang_getTypeDeclaration(typeCurrent);

                var cursorsAreEqual = clang_equalCursors(typeNextCursor, typeCurrentCursor) > 0;
                if (cursorsAreEqual)
                {
                    // union has a tag and a member name
                    continue;
                }
            }

            filteredFieldCursors.Add(current);
        }

        if (filteredFieldCursors.Count > 1)
        {
            filteredFieldCursors.Reverse();
        }

        return filteredFieldCursors.ToImmutableArray();
    }
}
