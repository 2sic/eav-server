﻿using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps
{
    public class AppDataConfiguration: IAppDataConfiguration
    {
        public AppDataConfiguration(bool showDrafts, bool publishingEnabled, ITokenListFiller configuration)
        {
            ShowDrafts = showDrafts;
            PublishingEnabled = publishingEnabled;
            Configuration = configuration;
        }

        public bool ShowDrafts { get; }

        public bool PublishingEnabled { get; }

        public ITokenListFiller Configuration { get; }

    }
}
