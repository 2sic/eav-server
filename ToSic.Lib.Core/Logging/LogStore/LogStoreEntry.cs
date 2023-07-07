using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    [PrivateApi]
    public class LogStoreEntry
    {
        public ILog Log { get; internal set; }
        public IDictionary<string, string> Specs { get; internal set; }

        public void UpdateSpecs(IDictionary<string, string> specs)
        {
            if (Specs == null) Specs = specs;
            // todo some day - merge specs
        }
    }
}
