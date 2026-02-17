using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.DataSource;

internal static class DataSourceOptionsExtensions
{
    internal static IDataSourceOptions WithAttach(this IDataSourceOptions? options, IDataSourceLinkable? attach)
    {
        return attach == null
            ? options ?? new DataSourceOptions
            {
                AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
            }
            : options.ToRecordOrNew(attach?.GetLink()?.DataSource?.PureIdentity() ?? null!) with { Attach = attach };

    }

    internal static DataSourceOptions ToRecordOrNew(this IDataSourceOptions? options, IAppIdentity appIdentityForNew) =>
        options switch
        {
            DataSourceOptions dataSourceOptions => dataSourceOptions,
            _ => new() { AppIdentityOrReader = appIdentityForNew, }
        };
}
