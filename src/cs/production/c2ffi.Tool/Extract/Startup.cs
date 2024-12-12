// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using c2ffi.Extract.Explore;
using c2ffi.Extract.Parse;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using ClangInstaller = c2ffi.Extract.Parse.ClangInstaller;
using ClangTranslationUnitParser = c2ffi.Extract.Parse.ClangTranslationUnitParser;
using Explorer = c2ffi.Extract.Explore.Explorer;
using ParseSystemIncludeDirectoriesProvider = c2ffi.Extract.Parse.ParseSystemIncludeDirectoriesProvider;

namespace c2ffi.Extract;

[UsedImplicitly]
internal sealed class Startup : IDependencyInjectionStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddSingleton<InputSanitizer>();

        _ = services.AddSingleton<ClangInstaller>();
        _ = services.AddSingleton<ClangTranslationUnitParser>();
        _ = services.AddSingleton<ParseArgumentsProvider>();
        _ = services.AddSingleton<ParseSystemIncludeDirectoriesProvider>();
        _ = services.AddSingleton<Explorer>();

        _ = services.AddTransient<ExploreContextFrontier>();

        _ = services.AddTransient<ArrayExplorer>();
        _ = services.AddTransient<EnumExplorer>();
        _ = services.AddTransient<FunctionExplorer>();
        _ = services.AddTransient<FunctionPointerExplorer>();
        _ = services.AddTransient<MacroObjectExplorer>();
        _ = services.AddTransient<OpaqueTypeExplorer>();
        _ = services.AddTransient<StructExplorer>();
        _ = services.AddTransient<UnionExplorer>();
        _ = services.AddTransient<TypeAliasExplorer>();
        _ = services.AddTransient<VariableExplorer>();
        _ = services.AddTransient<PointerExplorer>();
        _ = services.AddTransient<PrimitiveExplorer>();

        _ = services.AddSingleton<Command>();
        _ = services.AddSingleton<Tool>();
    }
}
