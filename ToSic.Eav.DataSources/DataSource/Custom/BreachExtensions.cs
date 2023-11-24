using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Special - very internal - helper to breach internal APIs in edge cases where they are needed outside.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class BreachExtensions
{
    public static CustomDataSource CustomDataSourceLight(CustomDataSource.MyServices services,
        IDataSource wrapper,
        string noParamOrder = Parameters.Protector,
        string logName = null)
    {
        var ds = new CustomDataSource(services, logName);
        ds.Error.Link(wrapper);
        ds.AutoLoadAllConfigMasks(wrapper.GetType());
        return ds;
    }

    public static IImmutableList<IEntity> TryGetIn(this CustomDataSource ds, string name = StreamDefaultName)
        => ds.TryGetIn(name);

    public static IImmutableList<IEntity> TryGetOut(this CustomDataSource ds, string name = StreamDefaultName)
        => ds.TryGetOut(name);

    public static void BreachProvideOut(
        this CustomDataSource ds,
        Func<object> source,
        string noParamOrder = Parameters.Protector,
        string name = StreamDefaultName,
        Func<DataFactoryOptions> options = default) =>
        ds.ProvideOut(source, options: options, name: name);
}