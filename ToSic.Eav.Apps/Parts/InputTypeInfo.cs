using System.Text.Json.Serialization;
using ToSic.Eav.Metadata;
using static ToSic.Eav.Metadata.Decorators;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Describes an input field type with it's labels, abilities etc. 
    /// This is so that input fields can self-describe.
    /// </summary>
    public class InputTypeInfo
    {
        public InputTypeInfo(string type, string label, string description, string assets, bool disableI18N, string ngAssets, bool useAdam, IMetadataOf metadata = null)
        {
            Type = type;
            Label = label;
            Description = description;
            Assets = assets;
            DisableI18n = disableI18N;
            AngularAssets = ngAssets;
            UseAdam = useAdam;

            if(metadata == null) return;

            if (metadata.HasType(IsObsoleteDecoratorId))
            {
                IsObsolete = true;
                ObsoleteMessage = metadata.GetBestValue<string>(MessageField, IsObsoleteDecoratorId);
            }
            if (metadata.HasType(RecommendedDecoratorId)) IsRecommended = true;

            if (metadata.HasType(IsDefaultDecorator)) IsDefault = true;
        }

        /// <summary>
        /// The type this input is for, like String etc.
        /// </summary>
        public string Type { get; }

        public string Label { get; }
        public string Description { get; }

        public string Assets { get; }

        #region new in 2sxc 10 / eav 5

        // ReSharper disable once InconsistentNaming
        public bool DisableI18n { get; }

        /// <summary>
        /// Additional resources to load (js/css)
        /// </summary>
        public string AngularAssets { get; }

        /// <summary>
        /// Activates ADAM in the UI for this input type
        /// </summary>
        public bool UseAdam { get; }

        /// <summary>
        /// WIP 12.10 Deprecation status of the input type - would always contain a message if it's deprecated
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? IsObsolete { get; set; }

        /// <summary>
        /// WIP 12.10 Deprecation status of the input type - would always contain a message if it's deprecated
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string ObsoleteMessage { get; set; }

        /// <summary>
        /// WIP 12.10 Recommendation status
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? IsRecommended { get; }

        /// <summary>
        /// WIP 12.10 Default selection status
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? IsDefault { get; }

        #endregion

    }
}
