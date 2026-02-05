using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using static ToSic.Eav.Apps.Sys.Work.WorkEntityRecycleBin;

namespace ToSic.Eav.DataSources;

[PrivateApi]
[VisualQuery(
    NiceName = "RecycleBin",
    NameId = "f890bec1-dee8-4ed6-9f2e-8ad412d2f4dc",
    NameIds =
    [
        "ToSic.Eav.DataSources.RecycleBin", // for use in the front end
    ],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System,
    UiHint = "RecycleBin in this application")]
// ReSharper disable once UnusedMember.Global
public class RecycleBin : CustomDataSource
{
    public RecycleBin(Dependencies services, GenWorkDb<WorkEntityRecycleBin> recycleBin)
        : base(services, logName: "CDS.RecycleBin", connect: [recycleBin])
    {
        ProvideOutRaw(
            () => GetList(recycleBin.New(AppId).Get()),
            options: () => new()
            {
                AutoId = true,
                TypeName = "RecycleBin",
            });
    }

    private IEnumerable<IRawEntity> GetList(IReadOnlyList<RecycleBinItem> recycleBinItems)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var list = recycleBinItems
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
                { AttributeNames.CreatedNiceName, r.DeletedUtc },
                { AttributeNames.ModifiedNiceName, r.DeletedUtc },
                { AttributeNames.TitleNiceName, $"{r.ContentTypeName}({r.EntityId})" },
            }))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }
}