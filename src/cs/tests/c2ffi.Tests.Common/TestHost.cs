// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tests.Library.Helpers;
using c2ffi.Tool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace c2ffi.Tests.Library;

#pragma warning disable CA1724
public static class TestHost
#pragma warning restore CA1724
{
    private static readonly IHost Host = GetHostBuilder().Build();

    public static IServiceProvider Services => Host.Services;

    private static IHostBuilder GetHostBuilder()
    {
        var result = new HostBuilder()
            .ConfigureHostCommon()
            .ConfigureServices(ConfigureTestServices);

        return result;
    }

    private static void ConfigureTestServices(IServiceCollection services)
    {
        _ = services.AddSingleton<FileSystemHelper>();
    }
}
