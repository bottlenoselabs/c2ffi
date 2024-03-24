// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Tool.Commands.Extract.Domain.Explore;
using c2ffi.Tool.Commands.Extract.Domain.Explore.NodeExplorers;
using c2ffi.Tool.Commands.Extract.Domain.Parse;
using c2ffi.Tool.Commands.Extract.Input;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using ClangInstaller = c2ffi.Tool.Commands.Extract.Domain.Parse.ClangInstaller;

namespace c2ffi.Tool.Commands.Extract;

[UsedImplicitly]
public sealed class Startup : IDependencyInjectionStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ExtractInputSanitizer>();

        services.AddSingleton<ClangInstaller>();
        services.AddSingleton<ClangTranslationUnitParser>();
        services.AddSingleton<ParseArgumentsProvider>();
        services.AddSingleton<ParseSystemIncludeDirectoriesProvider>();
        services.AddSingleton<Explorer>();

        services.AddTransient<ArrayExplorer>();
        services.AddTransient<EnumConstantExplorer>();
        services.AddTransient<EnumExplorer>();
        services.AddTransient<FunctionExplorer>();
        services.AddTransient<FunctionPointerExplorer>();
        services.AddTransient<MacroObjectExplorer>();
        services.AddTransient<OpaqueTypeExplorer>();
        services.AddTransient<StructExplorer>();
        services.AddTransient<UnionExplorer>();
        services.AddTransient<TypeAliasExplorer>();
        services.AddTransient<VariableExplorer>();
        services.AddTransient<PointerExplorer>();
        services.AddTransient<PrimitiveExplorer>();

        services.AddSingleton<ExtractFfiCommand>();
        services.AddSingleton<ExtractFfiTool>();
    }
}
