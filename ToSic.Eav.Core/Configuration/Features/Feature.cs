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
        public Guid Id { get; }

        /// <summary>
        /// Feature String ID
        /// </summary>
        [JsonProperty("sid")]
        public string Sid { get; }

        /// <summary>
        /// A nice name / title for showing in UIs
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// A nice description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; }


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


        public Feature(Guid id, bool? ui, bool? isPublic)
        {
            Id = id;
            Ui = ui;
            Public = isPublic;
        }

        public Feature(string sid, Guid guid,  string name, bool isPublic, bool ui, string description = "")
        {
            Id = guid;
            Sid = sid;
            Name = name;
            Public = isPublic;
            Ui = ui;
            Description = description;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original">This can be null!</param>
        /// <param name="guidIfUnknown">The feature guid, in case it's a feature which isn't known</param>
        public Feature(Feature original, Guid guidIfUnknown)
        {
            Id = original?.Id ?? guidIfUnknown;
            Sid = original?.Sid;
            Name = original?.Name;
            Public = original?.Public;
            Ui = original?.Ui;
            Description = original?.Description;
        }
    }
}
