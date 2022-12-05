using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys
{
    public class InstallAppsDto
    {
        public string remoteUrl { get; set; }

        public List<AppInstallRuleDto> rules { get; set; }

        public List<AppDtoLight> installedApps { get; set; }
    }

    public class AppInstallRuleDto
    {
        public string appGuid { get; set; }
        public string mode { get; set; }
        public string target { get; set; }
        public string url { get; set; }
    }

    public class AppDtoLight
    {
        public string name { get; set; }
        public string guid { get; set; }
        public string version { get; set; }
    }
}
