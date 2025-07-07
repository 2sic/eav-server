﻿namespace ToSic.Eav.DataSource;

partial class DataSourceBase: IDataSourceLinkable
{
    /// <inheritdoc />
    public virtual IDataSourceLink Link => _link.Get(() => new DataSourceLink(null, dataSource: this))!;
    private readonly GetOnce<IDataSourceLink> _link = new();
}