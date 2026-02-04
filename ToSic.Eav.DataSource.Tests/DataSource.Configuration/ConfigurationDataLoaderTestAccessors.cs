using ToSic.Eav.DataSource.Sys.Configuration;

namespace ToSic.Eav.DataSource.Configuration;

internal static class ConfigurationDataLoaderTestAccessors
{
    public static List<ConfigMaskInfo> GetTokensTac(this ConfigurationDataLoader loader, Type type)
        => loader.GetTokens(type);
}