using System.Collections.Generic;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Temporary solution so that our APIs use the IConfigurationSource
    /// and only need one object to transport all configurations.
    /// Not final. Very internal.
    /// </summary>
    public class ConfigurationWip: IConfiguration
    {
        public ILookUpEngine LookUpEngine { get; set; }

        public IDictionary<string, string> Values { get; set; }

        public ILookUpEngine GetLookupEngineWip() => LookUpEngine;

        public IDictionary<string, string> GetValuesWip() => Values;
    }
}
