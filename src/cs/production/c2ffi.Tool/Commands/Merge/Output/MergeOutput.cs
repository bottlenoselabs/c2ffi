// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs.Common.Tools;
using c2ffi.Tool.Commands.Merge.Input.Sanitized;
using JetBrains.Annotations;

namespace c2ffi.Tool.Commands.Merge.Output;

[UsedImplicitly]
public sealed class MergeOutput : ToolOutput<MergeInput>
{
    protected override void OnComplete()
    {
    }
}
