// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Data.Nodes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0130
namespace c2ffi.Extract.Explore;
#pragma warning restore IDE0130

[UsedImplicitly]
internal sealed class OpaqueTypeExplorer(ILogger<OpaqueTypeExplorer> logger)
    : NodeExplorer<COpaqueType>(logger, false)
{
    protected override KindCursors ExpectedCursors => KindCursors.Any;

    protected override KindTypes ExpectedTypes => KindTypes.Any;

    protected override CNode GetNode(ExploreContext exploreContext, NodeInfo info)
    {
        return OpaqueDataType(exploreContext, info);
    }

    private static COpaqueType OpaqueDataType(ExploreContext exploreContext, NodeInfo info)
    {
        var comment = exploreContext.Comment(info.ClangCursor);

        var result = new COpaqueType
        {
            Name = info.Name,
            Location = info.Location,
            Comment = comment
        };

        return result;
    }
}
