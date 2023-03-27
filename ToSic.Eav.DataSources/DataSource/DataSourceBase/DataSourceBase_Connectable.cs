using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSource
{
    public partial class DataSourceBase: IDataSourceLinkable
    {
        /// <inheritdoc />
        public IDataSourceLink Link => _link.Get(() => new DataSourceLink(null, dataSource: this));
        private readonly GetOnce<IDataSourceLink> _link = new GetOnce<IDataSourceLink>();
    }
}
