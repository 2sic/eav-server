namespace ToSic.Eav.Apps.Sys.PresetLoaders;

internal class AppContentTypesLoaderUnknown: ServiceBase, IAppContentTypesLoader, IIsUnknown
{
    public AppContentTypesLoaderUnknown(WarnUseOfUnknown<AppContentTypesLoaderUnknown> _) : base(LogScopes.NotImplemented + ".RepLdr") { }

    public IAppContentTypesLoader Init(IAppReader app, LogSettings logSettings)
    {
        Log.A("Unknown App Repo loader - won't load anything");
        return this;
    }

    public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => [];
}