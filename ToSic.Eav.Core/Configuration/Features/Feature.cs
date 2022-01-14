using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("no good reason to publish this")]
    public class Feature
    {
        private FeatureDefinition _featureDefinition;

        public Guid Guid => _featureDefinition.Guid;
        public string NameId => _featureDefinition.NameId;
        public string Name => _featureDefinition.Name;
        public string Description => _featureDefinition.Description;


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


        public bool Ui => _featureDefinition.Ui;
        public bool Public => _featureDefinition.Public;
        public FeatureSecurity Security => _featureDefinition.Security;


        public Feature(FeatureDefinition definition)
        {
            _featureDefinition = definition;
        }
        //public Feature(string nameId, Guid guid, string name, bool isPublic, bool ui, string description, FeatureSecurity security)
        //    : base(nameId, guid, name, isPublic, ui, description, security)
        //{
        //    //Guid = guid;
        //    //NameId = nameId;
        //    //Name = name;
        //    //Security = security;
        //    //Public = isPublic;
        //    //Ui = ui;
        //    //Description = description;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="original">This can be null!</param>
        ///// <param name="guidIfUnknown">The feature guid, in case it's a feature which isn't known</param>
        //public Feature(FeatureBase original, Guid guidIfUnknown)
        //    : base(original, guidIfUnknown)
        //{
        //    //Guid = original?.Guid ?? guidIfUnknown;
        //    //if(original == null) return;
        //    //Description = original.Description;
        //    //Name = original.Name;
        //    //NameId = original.NameId;
        //    //Public = original.Public;
        //    //Security = original.Security;
        //    //Ui = original.Ui;
        //}
    }
}
