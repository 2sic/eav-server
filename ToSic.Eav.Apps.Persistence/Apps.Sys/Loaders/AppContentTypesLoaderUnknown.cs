using ToSic.Eav.Apps.Sys.FileSystemState;
using ToSic.Eav.Apps.Sys.PresetLoaders;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Apps.Sys.Loaders;

internal class AppContentTypesLoaderUnknown: ServiceWithSetup<AppFileSystemLoaderOptions>, IAppContentTypesLoader, IIsUnknown
{
    public AppContentTypesLoaderUnknown(WarnUseOfUnknown<AppContentTypesLoaderUnknown> _) : base(LogScopes.NotImplemented + ".RepLdr") { }

    public void Init(AppFileSystemLoaderOptions options)
    {
        Log.A("Unknown App Repo loader - won't load anything");
    }

    public PartialData TypesAndEntities(IEntitiesSource entitiesSource) => new([], []);
}