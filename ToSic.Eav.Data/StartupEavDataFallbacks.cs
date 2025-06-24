﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Sys.Global;

namespace ToSic.Eav;

public static partial class StartupEavData
{
    /// <summary>
    /// This will add Do-Nothing services which will take over if they are not provided by the main system
    /// In general this will result in some features missing, which many platforms don't need or care about
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddEavDataFallbacks(this IServiceCollection services)
    {
        // At the moment there is nothing to add here
        services.TryAddTransient<IGlobalDataService, GlobalDataServiceUnknown>();

        return services;
    }
}