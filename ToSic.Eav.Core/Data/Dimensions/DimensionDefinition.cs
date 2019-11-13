using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// The definition of a dimension / language
    /// </summary>
    [PublicApi]
    public class DimensionDefinition
    {
        [PrivateApi]
        public int DimensionId { get; set; }

        [PrivateApi]
        public int? Parent { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// The id / marker in the environment
        /// </summary>
        [PrivateApi]
        public string EnvironmentKey { get; set; }

        public bool Matches(string environmentKey) => string.Equals(EnvironmentKey, environmentKey, StringComparison.InvariantCultureIgnoreCase);

        public bool Active { get; set; } = true;

    }
}
