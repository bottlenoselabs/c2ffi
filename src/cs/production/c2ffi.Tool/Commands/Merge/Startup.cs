// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tool.Commands.Merge.Input;
using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tool.Commands.Merge;

public sealed class Startup : IDependencyInjectionStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MergeFfisCommand>();
        services.AddSingleton<MergeFfisTool>();
        services.AddSingleton<MergeInputSanitizer>();
    }
}
