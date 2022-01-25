using System;
using System.Reflection;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Run
{
    [PrivateApi]
    public class SystemInformation
    {
        public static Version Version => _version ?? (_version = Assembly.GetExecutingAssembly().GetName().Version);
        private static Version _version;
    }
}
