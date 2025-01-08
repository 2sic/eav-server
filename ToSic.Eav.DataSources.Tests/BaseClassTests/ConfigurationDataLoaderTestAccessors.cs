using System.Collections.Generic;
using System;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal.Configuration;

namespace ToSic.Eav.DataSourceTests.BaseClassTests;

internal static class ConfigurationDataLoaderTestAccessors
{
    public static List<ConfigMaskInfo> GetTokensTac(this ConfigurationDataLoader loader, Type type)
        => loader.GetTokens(type);
}