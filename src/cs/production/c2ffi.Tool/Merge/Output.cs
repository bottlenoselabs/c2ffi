// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using bottlenoselabs.Common.Tools;
using JetBrains.Annotations;

namespace c2ffi.Merge;

[UsedImplicitly]
public sealed class Output : ToolOutput<InputSanitized>
{
    protected override void OnComplete()
    {
    }
}
