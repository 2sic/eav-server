using System;
using Newtonsoft.Json;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class Feature
    {
        /// <summary>
        /// Feature GUID
        /// </summary>
        [JsonProperty("id")]
        public Guid Id;

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

        /// <summary>
        /// If true, this feature will be provided to the Ui
        /// If null or false, it won't be given to the Ui
        /// </summary>
        /// <remarks>
        /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
        /// </remarks>
        [JsonProperty("ui", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ui { get; set; }

        /// <summary>
        /// If true, this feature will be provided to the Ui
        /// If null or false, it won't be given to the Ui
        /// </summary>
        /// <remarks>
        /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
        /// </remarks>
        [JsonProperty("public", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Public { get; set; }


        public Feature() { }

        public Feature(Guid id, bool? ui, bool? isPublic)
        {
            Id = id;
            Ui = ui;
            Public = isPublic;
        }
    }
}
