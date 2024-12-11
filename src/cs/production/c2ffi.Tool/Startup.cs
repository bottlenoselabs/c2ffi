// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace c2ffi.Tool;

#pragma warning disable CA1515
public static class Startup
#pragma warning restore CA1515
{
    [ExcludeFromCodeCoverage]
    public static IHost CreateHost(string[] args)
    {
        return new HostBuilder()
            .ConfigureDefaults(args)
            .UseConsoleLifetime()
            .ConfigureHostCommon()
            .Build();
    }

    public static IHostBuilder ConfigureHostCommon(this IHostBuilder builder)
    {
        return builder
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices);
    }

    private static void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        AddDefaultAppConfiguration(builder);
    }

    private static void AddDefaultAppConfiguration(IConfigurationBuilder builder)
    {
        var fileSystem = new FileSystem();
        var path = fileSystem.Path;
        var file = fileSystem.File;

        var originalSources = builder.Sources.ToImmutableArray();
        builder.Sources.Clear();

        var filePath = path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (file.Exists(filePath))
        {
            _ = builder.AddJsonFile(filePath);
        }
        else
        {
            var appSettingsResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith("appsettings.json", StringComparison.InvariantCulture));
            if (string.IsNullOrEmpty(appSettingsResourceName))
            {
                throw new MissingManifestResourceException("Missing appsettings.json embedded resource.");
            }

            var jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(appSettingsResourceName)!;
            _ = builder.AddJsonStream(jsonStream);
        }

        foreach (var originalSource in originalSources)
        {
            _ = builder.Add(originalSource);
        }
    }

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        _ = builder.ClearProviders();
        _ = builder.AddSimpleConsole();
        _ = builder.AddConfiguration(context.Configuration.GetSection("Logging"));
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddSingleton<IFileSystem, FileSystem>();
        _ = services.AddHostedService<CommandLineInterfaceHost>();
        _ = services.AddSingleton<CommandLineInterfaceCommand>();

        ConfigureStartupServices(services);
    }

    private static void ConfigureStartupServices(IServiceCollection services)
    {
        var interfaceType = typeof(IDependencyInjectionStartup);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => interfaceType.IsAssignableFrom(type) && type != interfaceType);
        foreach (var type in types)
        {
            var instance = (IDependencyInjectionStartup)Activator.CreateInstance(type)!;
            instance.ConfigureServices(services);
        }
    }
}
