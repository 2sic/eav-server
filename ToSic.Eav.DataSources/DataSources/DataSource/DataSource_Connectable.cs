using ToSic.Eav.DataSource;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources
{
    public partial class DataSource: IDataSourceLinkable
    {
        IDataSourceLink IDataSourceLinkable.Links => _link.Get(() => new DataSourceLink(null,
            dataSource: this
            //sourceName: DataSourceConstants.AllStreams,
            //targetName: DataSourceConstants.AllStreams
        ));
        private readonly GetOnce<IDataSourceLink> _link = new GetOnce<IDataSourceLink>();
    }
}
