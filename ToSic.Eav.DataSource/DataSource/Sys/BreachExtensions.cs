using ToSic.Eav.Data.Build;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSource.Sys;

/// <summary>
/// Special - very internal - helper to breach internal APIs in edge cases where they are needed outside.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class BreachExtensions
{
    public static IDataSourceLink CreateEmptyLink(IDataSource ds)
        => new DataSourceLink { DataSource = ds };

    public static CustomDataSource CustomDataSourceLight(CustomDataSource.Dependencies services,
        IDataSource wrapper,
        NoParamOrder npo = default,
        string? logName = null)
    {
        var ds = new CustomDataSource(services, logName);
        ds.Error.ConnectToParent(wrapper);
        ds.AutoLoadAllConfigMasks(wrapper.GetType(), services.ConfigDataLoader);
        return ds;
    }

    extension(CustomDataSource ds)
    {
        /// <inheritdoc cref="DataSourceBase.TryGetIn"/>
        public IImmutableList<IEntity>? TryGetIn(string name = StreamDefaultName)
            => ds.TryGetIn(name);

        /// <inheritdoc cref="DataSourceBase.TryGetOut"/>
        public IImmutableList<IEntity>? TryGetOut(string name = StreamDefaultName)
            => ds.TryGetOut(name);

        public void BreachProvideOut(Func<object> source,
            NoParamOrder npo = default,
            string name = StreamDefaultName,
            Func<DataFactoryOptions>? options = default
        ) => ds.ProvideOut(source, options: options, name: name);
    }
}