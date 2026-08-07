using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

internal class AppContentTypesLoaderUnknown: ServiceBase, IAppContentTypesLoader, IIsUnknown
{
    public AppContentTypesLoaderUnknown(WarnUseOfUnknown<AppContentTypesLoaderUnknown> _) : base(LogScopes.NotImplemented + ".RepLdr") { }

    public void Init(IAppReader app, ToSic.Sys.Logging.LogSettings logSettings, string? appFolderBeforeReaderIsReady = default)
    {
        Log.A("Unknown App Repo loader - won't load anything");
    }

    public PartialData TypesAndEntities(IEntitiesSource entitiesSource) => new([], []);
}