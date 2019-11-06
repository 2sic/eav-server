using ToSic.Eav.ValueProviders;

namespace ToSic.Eav.Apps
{
    public class AppDataConfiguration: IAppDataConfiguration
    {
        public AppDataConfiguration(bool showDrafts, bool versioningEnabled, IValueCollectionProvider configuration)
        {
            ShowDrafts = showDrafts;
            VersioningEnabled = versioningEnabled;
            Configuration = configuration;
        }

        public bool ShowDrafts { get; }

        public bool VersioningEnabled { get; }

        public IValueCollectionProvider Configuration { get; }

    }
}
