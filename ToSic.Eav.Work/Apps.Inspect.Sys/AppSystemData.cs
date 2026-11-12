using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.WebApi.Sys.Admin;
// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.Apps.Inspect.Sys;

/// <summary>
/// All data needed by the App administration settings screen in one request.
/// </summary>
[PrivateApi]
[VisualQuery(
    NiceName = "App System Data",
    NameId = "bbcd8a91-1ddb-4d27-b7b6-ca6f5cebdbae",
    NameIds = ["System.AppSystemData"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Settings, resources, fields, app configuration and metadata of the current app"
)]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppSystemData(
    CustomDataSource.Dependencies services,
    AppWorkContextService appWorkCtxSvc,
    AppWorkChain<WorkAttributes> workAttributes,
    Generator<ConvertAttributeToRaw> convertAttribute,
    IAppsCatalog appsCatalog,
    AppWorkQuick<EntitiesAdminData> entitiesAdminData,
    AppWorkChain<WorkMetadataGet> metadataController)
    : AppEnhancements(
        services,
        appWorkCtxSvc,
        workAttributes,
        convertAttribute,
        appsCatalog,
        metadataController)
{
    protected override IEnumerable<IEntity> GetEntities(string? typeName)
        => entitiesAdminData.New(AppId).GetEntities(typeName);
}
