using ToSic.Eav.DataSources.Linking;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources
{
    public partial class DataSource: IDataSourceLink
    {
        IDataSourceLinkInfo IDataSourceLink.Link => _link.Get(() => new DataSourceLinkInfo(null,
            dataSource: this
            //sourceName: DataSourceConstants.AllStreams,
            //targetName: DataSourceConstants.AllStreams
        ));
        private readonly GetOnce<IDataSourceLinkInfo> _link = new GetOnce<IDataSourceLinkInfo>();
    }
}
