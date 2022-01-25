using System;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// This stores the enabled / expiry of a feature
    /// </summary>
    [PrivateApi("no good reason to publish this")]
    public class FeatureConfig
    {
        /// <summary>
        /// Feature GUID
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Feature is enabled and hasn't expired yet
        /// </summary>
        /// <remarks>by default all features are disabled</remarks>
        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => _enabled && Expires > DateTime.Now;
            set => _enabled = value;
        }
        private bool _enabled;

        /// <summary>
        /// Expiry of this feature
        /// </summary>
        [JsonProperty("expires")]
        public DateTime Expires { get; set; } = DateTime.MaxValue;

    }
}
