using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Sys;
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

    public RecycleBin(Dependencies services, AppWorkQuick<WorkEntityRecycleBin> recycleBin, FeaturesForDataSources featuresForDs)
        : base(services, logName: "CDS.RecycleBin", connect: [recycleBin, featuresForDs])
    {
        var bin = new Lazy<WorkEntityRecycleBin>(() => recycleBin.New(this.PureIdentity()));
        ProvideOutRaw(
            () => GetList(bin.Value).Entities,
            options: () => new()
            {
                TypeName = "RecycleBin",
            });

        ProvideOutRaw(
            () => GetList(bin.Value).ContentTypes,
            name: "ContentTypes",
            options: () => new()
            {
                AutoId = true,
                TypeName = "ContentTypes",
            });

        // Feature State / Status
        ProvideOut(name: FeaturesForDataSources.StreamName,
            data: () => featuresForDs.GetDataForFeature(BuiltInFeatures.EntityUndelete));
    }

    private (IEnumerable<IRawEntity> Entities, IEnumerable<IRawEntity> ContentTypes) _cache = (null!, null!); // temp null at start

    private (IEnumerable<IRawEntity> Entities, IEnumerable<IRawEntity> ContentTypes) GetList(WorkEntityRecycleBin recycleBin)
    {
        var l = Log.Fn<(IEnumerable<IRawEntity> Entities, IEnumerable<IRawEntity> ContentTypes)>($"DateFrom:{DateFrom}, DateTo:{DateTo}, ContentType:{ContentType}");

        if (_cache.Entities != null)
            return l.Return(_cache, "from cache");

        var items = recycleBin.Get(DateFrom, DateTo);

        var ct = ContentType;
        var itemsOfContentType = string.IsNullOrEmpty(ct)
        ? items
        : items.Where(i => i.TypeName.EqualsInsensitive(ct));

        // TODO: @2rb - make the underlying RecycleBinItem an AutoConvert with all necessary properties
        // and then drop the options on top
        var list = itemsOfContentType
            .Select(r => new RawEntity
            {
                Id = r.Id,
                Guid = r.Guid,
                Values = new Dictionary<string, object?>
                {
                    { nameof(r.AppId), r.AppId },
                    { nameof(r.TypeNameId), r.TypeNameId },
                    { nameof(r.TypeName), r.TypeName },
                    { nameof(r.TransactionId), r.TransactionId },
                    { nameof(r.Deleted), r.Deleted },
                    { nameof(r.DeletedBy), r.DeletedBy },
                    { nameof(r.ParentRef), r.ParentRef },
                    { nameof(r.Json), r.Json },
                    { AttributeNames.TitleNiceName, $"{r.TypeName} ({r.Id})" },
                },
            })
            .ToList();

        // TODO: @2rb - make this a Raw-AutoConvert with all necessary properties
        // and then drop the options on top
        var contentTypes = items
            .GroupBy(i => i.TypeNameId)
            .OrderBy(c => c.Key)
            .Select(c =>
            {
                var first = c.First();
                return new RawEntity
                {
                    Values = new Dictionary<string, object?>
                    {
                        { AttributeNames.TitleNiceName, $"{first.TypeName} ({c.Count()})" },
                        { "Name", first.TypeName },
                        { AttributeNames.NameIdNiceName, first.TypeNameId },
                        { "Count", c.Count() },
                    }
                };
            })
            .ToList();

        _cache = (list, contentTypes);

        return l.Return(_cache, $"entities:{list.Count}, contentTypes:{contentTypes.Count}");
    }
}