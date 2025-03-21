﻿using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Lib;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartUp
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IServiceCollection AddLibCore(this IServiceCollection services) =>
        services
            .AddLibLogging()
            .AddLibDiServiceSwitchers()
            .AddLibDiBasics();
}