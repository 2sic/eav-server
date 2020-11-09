using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps
{
    public class AppDataConfiguration: IAppDataConfiguration
    {
        public AppDataConfiguration(bool showDrafts, ILookUpEngine configuration)
        {
            ShowDrafts = showDrafts;
            Configuration = configuration;
        }

        public bool ShowDrafts { get; }


        public ILookUpEngine Configuration { get; }

    }
}
