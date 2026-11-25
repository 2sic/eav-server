using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Data.ContentTypes.Fields.Sys;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;
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
public class AppSystemData : CustomDataSource
{
    public AppSystemData(
        Dependencies services,
        AppWorkContextService workContextService,
        AppWorkChain<WorkAttributes> workAttributes,
        Generator<ConvertAttributeToRaw> convertAttribute,
        IAppsCatalog appsCatalog,
        AppWorkQuick<EntitiesAdminData> entitiesAdminData,
        AppWorkChain<WorkMetadataGet> workMetadata)
        : base(services, logName: "Sxc.AppSys", connect:
        [
            workContextService,
            workAttributes,
            convertAttribute,
            appsCatalog,
            entitiesAdminData,
            workMetadata,
        ])
    {
        var context = new Lazy<IAppWorkContext>(() => workContextService.ContextNew(AppId));
        var attributes = new Lazy<WorkAttributes>(() => workAttributes.New(context.Value));
        var fieldConverter = new Lazy<ConvertAttributeToRaw>(() => convertAttribute.New().Init(AppId, false));
        var typeNames = new Lazy<(string? Settings, string? Resources)>(() => GetTypeNames(appsCatalog, context.Value));

        ProvideOut(() => GetEntities(entitiesAdminData, typeNames.Value.Settings), "AppSettings");

        ProvideOut(() => GetEntities(entitiesAdminData, typeNames.Value.Resources), "AppResources");

        ProvideOut(
            () => GetEntities(entitiesAdminData, AppStackConstants.Settings.SystemType),
            AppStackConstants.Settings.SystemType
        );

        ProvideOut(
            () => GetEntities(entitiesAdminData, AppStackConstants.Resources.SystemType),
            AppStackConstants.Resources.SystemType
        );

        ProvideOut(() => GetEntities(entitiesAdminData, AppLoadConstants.TypeAppConfig), "AppConfig");

        ProvideOutRaw(
            () => GetFields(attributes.Value, fieldConverter.Value, typeNames.Value.Settings),
            name: "AppSettingFields"
        );

        ProvideOutRaw(
            () => GetFields(attributes.Value, fieldConverter.Value, typeNames.Value.Resources),
            name: "AppResourcesFields"
        );

        ProvideOutRaw(
            () => GetMetadata(workMetadata.New(context.Value)),
            name: "Metadata",
            options: () => new() { AllowUnknownValueTypes = true }
        );
    }

    private IEnumerable<IEntity> GetEntities(AppWorkQuick<EntitiesAdminData> entitiesAdminData, string? typeName)
        => entitiesAdminData.New(AppId).GetEntities(typeName);

    private static IEnumerable<ContentTypeFieldRaw> GetFields(
        WorkAttributes attributes,
        ConvertAttributeToRaw converter,
        string? typeName)
    {
        if (typeName == null)
            return [];

        return converter.Convert(attributes.GetFields(typeName));
    }

    private IEnumerable<IRawEntity> GetMetadata(WorkMetadataGet workMetadata)
    {
        var items = workMetadata
            .Get(AppId, (int)TargetTypes.App, "number", AppId.ToString())
            .Items ?? [];

        return items.Select(item => new RawEntity
        {
            Id = item.TryGetValue("Id", out var id) ? Convert.ToInt32(id) : 0,
            Values = item.ToDictionary(pair => pair.Key, pair => pair.Value),
        });
    }

    private (string? Settings, string? Resources) GetTypeNames(IAppsCatalog appsCatalog, IAppDataContext context)
    {
        var app = appsCatalog.AppIdentity(AppId);
        var isPrimary = app.AppId == appsCatalog.PrimaryAppIdentity(app.ZoneId).AppId;

        if (!app.IsGlobalSettingsApp() && !isPrimary)
            return (AppLoadConstants.TypeAppSettings, AppLoadConstants.TypeAppResources);

        // Global and primary apps only expose settings/resources if their special type exists.
        var hasSettings = HasConfigurationType(context, AppStackConstants.Settings.CustomType);
        var hasResources = HasConfigurationType(context, AppStackConstants.Resources.CustomType);

        return (
            hasSettings ? AppStackConstants.Settings.CustomType : null,
            hasResources ? AppStackConstants.Resources.CustomType : null
        );
    }

    private static bool HasConfigurationType(IAppDataContext context, string typeName)
        => context.AppReader.ContentTypes.Any(type =>
            type.Scope == ScopeConstants.SystemConfiguration && type.Name == typeName);
}
