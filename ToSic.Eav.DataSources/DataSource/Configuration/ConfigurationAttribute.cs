using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    /// <summary>
    /// Marks an attribute of a DataSource as a `Configuration` attribute.
    /// This means that the internal system which loads configurations from a config-entity will automatically retrieve the value
    /// as specified.
    ///
    /// Note that 
    ///
    /// **Usage**
    ///
    /// * `[ConfigurationData]` - simple case, just get it from configuration, no fallback
    /// * `[ConfigurationData(Fallback = True)]`
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP v15.03")]
    [AttributeUsage(validOn: AttributeTargets.Property)]
    public class ConfigurationAttribute: Attribute
    {
        /// <summary>
        /// Default, empty constructor.
        /// All properties must be added in a named fashion to ensure long-term API consistency.
        /// </summary>
        public ConfigurationAttribute() { }

        /// <summary>
        /// The Token is the most complicated way to create a field mask.
        /// It must have the full syntax inside `[...]`.
        /// Examples:
        ///
        /// * `[Source:Key]`
        /// * `[Source:Key|format]`
        /// * `[Source:Key||Fallback]`
        /// * `[Source:Key|format|Fallback]`
        /// * `[Source:Key||[SubSource:SubKey||FinalFallback]]`
        /// * etc.
        /// </summary>
        public string Token { get; set; }

        public string Field { get; set; }

        public object Fallback { get; set; }

        public bool CacheRelevant { get; set; } = true;
    }
}
