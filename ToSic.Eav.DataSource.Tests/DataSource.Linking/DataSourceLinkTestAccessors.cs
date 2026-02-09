namespace ToSic.Eav.DataSource.Linking;

public static class DataSourceLinkTestAccessors
{
    public static string OutNameTac(this IDataSourceLink link) => link.OutName;
    
    public static string InNameTac(this IDataSourceLink link) => link.InName;

    public static IDataSource? DataSourceTac(this IDataSourceLink link) => link.DataSource;

    public static IEnumerable<IDataSourceLink> MoreTac(this IDataSourceLink link) => link.More;

    public static IDataSourceLink WithRenameTac(this IDataSourceLink link, string? outName = default, string? inName = default) =>
        link.WithRename(outName, inName);

    public static IDataSourceLink WithAnotherStreamTac(this IDataSourceLink link, string? name = default,
        string? outName = default, string? inName = default) =>
        link.WithAnotherStream(name, outName, inName);

    public static IDataSourceLink WithMoreTac(this IDataSourceLink link, IDataSourceLinkable[] more) =>
        link.WithMore(more);

    public static IEnumerable<IDataSourceLink> FlattenTac(this IDataSourceLink link) =>
        link.Flatten();
}
