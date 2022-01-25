using System;
using System.Reflection;

namespace ToSic.Eav
{
    public class EavSystemInfo
    {

        public static readonly string VersionString = VersionToNiceFormat(Assembly.GetExecutingAssembly().GetName().Version);

        // Todo: probably move to plumbing or extension method?
        public static string VersionToNiceFormat(Version version)
            => $"{version.Major:00}.{version.Minor:00}.{version.Build:00}";

    }
}
