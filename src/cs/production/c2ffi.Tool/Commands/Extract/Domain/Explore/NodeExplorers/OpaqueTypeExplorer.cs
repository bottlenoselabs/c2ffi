// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;

[UsedImplicitly]
public sealed class OpaqueTypeExplorer(ILogger<OpaqueTypeExplorer> logger)
    : NodeExplorer<COpaqueType>(logger, false)
{
    protected override ExploreKindCursors ExpectedCursors => ExploreKindCursors.Any;

    protected override ExploreKindTypes ExpectedTypes => ExploreKindTypes.Any;

    protected override CNode GetNode(ExploreContext context, ExploreCandidateInfoNode info)
    {
        return OpaqueDataType(context, info);
    }

    private static COpaqueType OpaqueDataType(ExploreContext context, ExploreCandidateInfoNode info)
    {
        var comment = context.Comment(info.Cursor);
        var isSystemCursor = context.IsSystemCursor(info.Cursor);

        var result = new COpaqueType
        {
            Name = info.Name,
            Location = info.Location,
            SizeOf = info.SizeOf!.Value,
            Comment = comment,
            IsSystem = isSystemCursor
        };

        return result;
    }
}
