using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Sys;
using ToSic.Eav.Repositories.Sys;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Utils;

namespace ToSic.Eav.DataSources;

[PrivateApi]
[VisualQuery(
    NiceName = "RecycleBin",
    NameId = "f890bec1-dee8-4ed6-9f2e-8ad412d2f4dc",
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System,
    UiHint = "RecycleBin in this application")]
// ReSharper disable once UnusedMember.Global
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class RecycleBin : CustomDataSource
{
    #region Configuration properties

    [Configuration]
    public DateTime? DateFrom => Configuration.GetThis(default(DateTime?));

    [Configuration]
    public DateTime? DateTo => Configuration.GetThis(default(DateTime?));

    [Configuration]
    public string? ContentType => Configuration.GetThis();

    #endregion

    public RecycleBin(Dependencies services, LazySvc<IAppReaderFactory> apps, Generator<IStoreRecycleBin, IAppReader> recycleBin, FeaturesForDataSources featuresForDs)
        : base(services, logName: "CDS.RecycleBin", connect: [recycleBin, apps, featuresForDs])
    {
        var bin = new Lazy<IStoreRecycleBin>(() => recycleBin.New(apps.Value.Get(this.PureIdentity())));
        ProvideOutRaw(() => GetList(bin.Value).Entities);

        ProvideOutRaw(
            () => GetList(bin.Value).ContentTypes,
            name: "ContentTypes");

        // Feature State / Status
        ProvideOut(name: FeaturesForDataSources.StreamName,
            data: () => featuresForDs.GetDataForFeature(BuiltInFeatures.EntityUndelete));
    }

    private (IEnumerable<IStoreRecycleBin.RecycleBinItem> Entities, IEnumerable<RecycleBinContentType> ContentTypes) _cache = (null!, null!); // temp null at start

    private (IEnumerable<IStoreRecycleBin.RecycleBinItem> Entities, IEnumerable<RecycleBinContentType> ContentTypes) GetList(IStoreRecycleBin recycleBin)
    {
        var l = Log.Fn<(IEnumerable<IStoreRecycleBin.RecycleBinItem> Entities, IEnumerable<RecycleBinContentType> ContentTypes)>($"DateFrom:{DateFrom}, DateTo:{DateTo}, ContentType:{ContentType}");

        if (_cache.Entities != null)
            return l.Return(_cache, "from cache");

        var items = recycleBin.Get(new(DateFrom, DateTo, ContentType.NullIfNoValue()));

        var contentTypes = items
            .GroupBy(i => i.TypeNameId)
            .OrderBy(c => c.Key)
            .Select(c =>
            {
                var first = c.First();
                return new RecycleBinContentType(first.TypeName, first.TypeNameId, c.Count());
            })
            .ToList();

        _cache = (items, contentTypes);

        return l.Return(_cache, $"entities:{items.Count}, contentTypes:{contentTypes.Count}");
    }

    [ContentType(
        Name = "ContentTypes",
        Guid = "41cd6de4-556f-47a1-9cc5-6acbbca6fd51",
        Description = "Content type represented in the recycle bin",
        Scope = "System"
    )]
    private sealed record RecycleBinContentType(string Name, string NameId, int Count) : IRawEntityAutoConvert
    {
        [ContentTypeTitle]
        public string Title => $"{Name} ({Count})";
    }
}
