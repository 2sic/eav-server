using ToSic.Eav.Apps.Sys;
using ToSic.Eav.DataSource.DataProviders;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.Data.Versions.Sys;

[PrivateApi]
[VisualQuery(
    NiceName = "Item History",
    NameId = "43f0261c-7400-4edb-b622-2606db9eebda",
    NameIds = ["System.ItemHistory"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Confidential,
    UiHint = "Version history of an item")]
// ReSharper disable once UnusedMember.Global
public class ItemHistory(CustomDataSource.Dependencies services, AppWorkQuick<RawDataProviderBroker> data)
    : CustomDataSource(services, "Sxc.ItemHist", connect: [data])
{
    [Configuration]
    public int EntityId => Configuration.GetThis(0);

    protected override IEnumerable<IRawData> GetDefault()
        => data.New(this.PureIdentity())
            .Provide(WorkEntityVersionsGet.NameId, Configuration);

}
