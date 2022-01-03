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
        //[JsonProperty("id")]
        public Guid Guid { get; }

        /// <summary>
        /// Feature String ID
        /// </summary>
        public string NameId { get; }

        /// <summary>
        /// A nice name / title for showing in UIs
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A nice description
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// Feature is enabled and hasn't expired yet
        /// </summary>
        /// <remarks>by default all features are disabled</remarks>
        //[JsonProperty("enabled")]
        public bool Enabled
        {
            get => _enabled && Expires > DateTime.Now;
            set => _enabled = value;
        }
        private bool _enabled;

        /// <summary>
        /// Expiry of this feature
        /// </summary>
        //[JsonProperty("expires")]
        public DateTime Expires { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// If true, this feature will be provided to the Ui
        /// If null or false, it won't be given to the Ui
        /// </summary>
        /// <remarks>
        /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ui { get; set; }

        /// <summary>
        /// If true, this feature will be provided to the Ui
        /// If null or false, it won't be given to the Ui
        /// </summary>
        /// <remarks>
        /// This has to do with load-time and security. We don't want to broadcast every feature to the Ui.
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Public { get; set; }


        public Feature(Guid guid, bool? ui, bool? isPublic)
        {
            Guid = guid;
            Ui = ui;
            Public = isPublic;
        }

        public Feature(string nameId, Guid guid,  string name, bool isPublic, bool ui, string description = "")
        {
            Guid = guid;
            NameId = nameId;
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
            Guid = original?.Guid ?? guidIfUnknown;
            NameId = original?.NameId;
            Name = original?.Name;
            Public = original?.Public;
            Ui = original?.Ui;
            Description = original?.Description;
        }
    }
}
