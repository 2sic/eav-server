﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Code.InfoSystem;
using CodeInfoService = ToSic.Sys.Code.InfoSystem.CodeInfoService;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupSysCode
{
    public static IServiceCollection AddSysCode(this IServiceCollection services)
    {
        // V16.02 - Obsolete service
        services.TryAddTransient<CodeInfoService>();
        services.TryAddScoped<CodeInfosInScope>();
        services.TryAddTransient<CodeInfoStats>();

        return services;
    }

}