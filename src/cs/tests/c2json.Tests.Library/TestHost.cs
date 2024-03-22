// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2json.Tests.Library.Helpers;
using c2json.Tool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration
[assembly: CollectionBehavior(MaxParallelThreads = 1)]

namespace c2json.Tests;

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
        services.AddSingleton<FileSystemHelper>();
    }
}
