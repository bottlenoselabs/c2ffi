// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace c2json.Tool.Commands.Merge;

[UsedImplicitly]
public sealed class Startup : IDependencyInjectionStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MergeFfisCommand>();
    }
}
