// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Merge;

[UsedImplicitly]
internal sealed class Startup : IDependencyInjectionStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddSingleton<Command>();
        _ = services.AddTransient<Tool>();
        _ = services.AddSingleton<InputSanitizer>();
    }
}
