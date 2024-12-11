// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tool.Commands.Merge.Input;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tool.Commands.Merge;

[UsedImplicitly]
internal sealed class Startup : IDependencyInjectionStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddSingleton<MergeFfisCommand>();
        _ = services.AddTransient<MergeFfisTool>();
        _ = services.AddSingleton<MergeInputSanitizer>();
    }
}
