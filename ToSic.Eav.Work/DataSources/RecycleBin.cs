using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Sys;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Utils;
using static ToSic.Eav.Apps.Sys.Work.WorkEntityRecycleBin;

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

    public RecycleBin(Dependencies services, GenWorkDb<WorkEntityRecycleBin> recycleBin, FeaturesForDataSources featuresForDs)
        : base(services, logName: "CDS.RecycleBin", connect: [recycleBin, featuresForDs])
    {
        ProvideOutRaw(
            () => GetList(recycleBin).Entities,
            options: () => new()
            {
                AutoId = true,
                TypeName = "RecycleBin",
            });

        ProvideOutRaw(
            () => GetList(recycleBin).ContentTypes,
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

    private (IEnumerable<IRawEntity> Entities, IEnumerable<IRawEntity> ContentTypes) _cache = (null, null);

    private (IEnumerable<IRawEntity> Entities, IEnumerable<IRawEntity> ContentTypes) GetList(GenWorkDb<WorkEntityRecycleBin> recycleBin)
    {
        var l = Log.Fn<(IEnumerable<IRawEntity> Entities, IEnumerable<IRawEntity> ContentTypes)>($"DateFrom:{DateFrom}, DateTo:{DateTo}, ContentType:{ContentType}");

        if (_cache.Entities != null)
            return l.Return(_cache, "from cache");

        var items = recycleBin.New(AppId)
            .Get(DateFrom, DateTo);

        var ct = ContentType;
        var itemsOfContentType = string.IsNullOrEmpty(ct)
        ? items
        : items.Where(i => i.ContentTypeName.EqualsInsensitive(ct));

        var list = itemsOfContentType
            .Select(r => new RawEntity(new()
            {
                { nameof(r.EntityId), r.EntityId },
                { nameof(r.EntityGuid), r.EntityGuid.ToString() },
                { nameof(r.AppId), r.AppId },
                { nameof(r.ContentTypeStaticName), r.ContentTypeStaticName },
                { nameof(r.ContentTypeName), r.ContentTypeName },
                { nameof(r.DeletedTransactionId), r.DeletedTransactionId },
                { nameof(r.DeletedUtc), r.DeletedUtc },
                { nameof(r.DeletedBy), r.DeletedBy },
                { nameof(r.ParentRef), r.ParentRef },
                { nameof(r.Json), r.Json },
                { nameof(r.FilterDateFrom), r.FilterDateFrom },
                { nameof(r.FilterDateTo), r.FilterDateTo },
                { nameof(r.FilterContentType), r.FilterContentType },
                { AttributeNames.CreatedNiceName, r.DeletedUtc },
                { AttributeNames.ModifiedNiceName, r.DeletedUtc },
                { AttributeNames.TitleNiceName, $"{r.ContentTypeName}({r.EntityId})" },
            }))
            .ToList();

        var contentTypes = items
            .GroupBy(i => new { i.ContentTypeStaticName, i.ContentTypeName })
            .OrderBy(c => c.Key.ContentTypeName)
            .Select(c => new RawEntity(new()
            {
                { "Name", c.Key.ContentTypeName },
                { "StaticName", c.Key.ContentTypeStaticName },
                { "Count", c.Count() },
                { AttributeNames.TitleNiceName, $"{c.Key.ContentTypeName} ({c.Count()})" },
            }))
            .ToList();

        _cache = (list, contentTypes);

        return l.Return(_cache, $"entities:{list.Count}, contentTypes:{contentTypes.Count}");
    }
}