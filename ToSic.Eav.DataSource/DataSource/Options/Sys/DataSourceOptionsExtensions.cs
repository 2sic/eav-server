using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.DataSource;

public static class DataSourceOptionsExtensions
{
    public static IDataSourceOptions WithAttach(this IDataSourceOptions? options, IDataSourceLinkable? attach)
    {
        return attach == null
            ? options ?? DataSourceOptions.Empty()
            : options.ToRecordOrNew(attach?.GetLink()?.DataSource?.PureIdentity() ?? null!) with { Attach = attach };

    }

    internal static DataSourceOptions ToRecordOrNew(this IDataSourceOptions? options, IAppIdentity appIdentityForNew) =>
        options switch
        {
            DataSourceOptions dataSourceOptions => dataSourceOptions,
            _ => new() { AppIdentityOrReader = appIdentityForNew, }
        };
}
