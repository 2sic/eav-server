using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "Entities Admin",
    NameId = "7dd4fe46-7a83-4cc6-b3d3-d506b335b290",
    NameIds = ["System.EntitiesAdmin"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Admin list of entities for a content type"
)]
public class EntitiesAdmin : DataSourceBase
{
    #region Configuration Properties

    /// <summary>
    /// The static name of the content type.
    /// </summary>
    [Configuration(Fallback = "")]
    public string ContentType => Configuration.GetThis(fallback: "");

    #endregion

    public EntitiesAdmin(Dependencies services, LazySvc<EntitiesAdminData> entitiesAdminData)
        : base(services, logName: "Eav.EntitiesAdmin", connect: [entitiesAdminData])
    {
        ProvideOut(() => entitiesAdminData.Value.GetEntities(AppId, ContentType));
    }
}
