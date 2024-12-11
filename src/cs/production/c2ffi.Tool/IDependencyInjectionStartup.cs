// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace c2ffi.Tool;

internal interface IDependencyInjectionStartup
{
    void ConfigureServices(IServiceCollection services);
    // void Configure(IApplicationBuilder app, IWebHostEnvironment env);
}
