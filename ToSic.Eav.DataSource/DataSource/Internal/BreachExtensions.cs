﻿using ToSic.Eav.Data.Build;
using ToSic.Lib.Coding;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSource.Internal;

/// <summary>
/// Special - very internal - helper to breach internal APIs in edge cases where they are needed outside.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class BreachExtensions
{
    public static IDataSourceLink CreateEmptyLink(IDataSource ds) => new DataSourceLink(null, dataSource: ds);

    public static CustomDataSource CustomDataSourceLight(CustomDataSource.MyServices services,
        IDataSource wrapper,
        NoParamOrder noParamOrder = default,
        string? logName = null)
    {
        var ds = new CustomDataSource(services, logName);
        ds.Error.ConnectToParent(wrapper);
        ds.AutoLoadAllConfigMasks(wrapper.GetType());
        return ds;
    }

    public static IImmutableList<IEntity>? TryGetIn(this CustomDataSource ds, string name = StreamDefaultName)
        => ds.TryGetIn(name);

    public static IImmutableList<IEntity>? TryGetOut(this CustomDataSource ds, string name = StreamDefaultName)
        => ds.TryGetOut(name);

    public static void BreachProvideOut(
        this CustomDataSource ds,
        Func<object> source,
        NoParamOrder noParamOrder = default,
        string name = StreamDefaultName,
        Func<DataFactoryOptions>? options = default) =>
        ds.ProvideOut(source, options: options, name: name);
}