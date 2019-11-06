using ToSic.Eav.LookUp;
using ToSic.Eav.ValueProviders;

namespace ToSic.Eav.Apps
{
    public class AppDataConfiguration: IAppDataConfiguration
    {
        public AppDataConfiguration(bool showDrafts, bool versioningEnabled, ITokenListFiller configuration)
        {
            ShowDrafts = showDrafts;
            VersioningEnabled = versioningEnabled;
            Configuration = configuration;
        }

        public bool ShowDrafts { get; }

        public bool VersioningEnabled { get; }

        public ITokenListFiller Configuration { get; }

    }
}
