using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

internal class AppContentTypesLoaderUnknown: ServiceBase, IAppContentTypesLoader, IIsUnknown
{
    public AppContentTypesLoaderUnknown(WarnUseOfUnknown<AppContentTypesLoaderUnknown> _) : base(LogScopes.NotImplemented + ".RepLdr") { }

    public void Init(IAppReader app, LogSettings logSettings, string? appFolderBeforeReaderIsReady = default)
    {
        Log.A("Unknown App Repo loader - won't load anything");
    }

    public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => [];
}