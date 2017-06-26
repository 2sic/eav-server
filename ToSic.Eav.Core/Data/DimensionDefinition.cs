using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Manages which dimensions (languages) exist on a zone
    /// Note that there is a mapping environment-key and systemkey
    /// </summary>
    public class DimensionDefinition
    {
        public int DimensionId { get; set; }
        public int? Parent { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string EnvironmentKey { get; set; }

        public bool Matches(string environmentKey) => string.Equals(EnvironmentKey, environmentKey, StringComparison.InvariantCultureIgnoreCase);

        public bool Active { get; set; } = true;

    }
}
