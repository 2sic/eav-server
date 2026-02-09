namespace ToSic.Eav.DataSource;

partial class DataSourceBase: IDataSourceLinkable
{
    /// <inheritdoc />
    public virtual IDataSourceLink GetLink() => _link ??= new DataSourceLink { DataSource = this };
    private IDataSourceLink? _link;
}