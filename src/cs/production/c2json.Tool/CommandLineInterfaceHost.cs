// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.CommandLine;
using Microsoft.Extensions.Hosting;

namespace c2json.Tool;

public sealed class CommandLineInterfaceHost(
    IHostApplicationLifetime applicationLifetime,
    CommandLineInterfaceCommand command)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        applicationLifetime.ApplicationStarted.Register(() => Task.Run(Main, cancellationToken));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void Main()
    {
        var commandLineArguments = Environment.GetCommandLineArgs().Skip(1).ToArray();

        try
        {
            Environment.ExitCode = command.Invoke(commandLineArguments);
        }
        catch
        {
            Environment.ExitCode = 1;
            throw;
        }

        applicationLifetime.StopApplication();
    }
}
