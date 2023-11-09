using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;
using static System.StringComparer;
using static ToSic.Eav.Metadata.Decorators;

namespace ToSic.Eav.Apps.Work
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

            if (metadata == null) return;

            if (metadata.HasType(IsObsoleteDecoratorId))
            {
                IsObsolete = true;
                ObsoleteMessage = metadata.GetBestValue<string>(MessageField, IsObsoleteDecoratorId);
            }
            if (metadata.HasType(RecommendedDecoratorId)) IsRecommended = true;

            if (metadata.HasType(IsDefaultDecorator)) IsDefault = true;

            var typeInputTypeDef = metadata.FirstOrDefaultOfType(InputTypes.TypeForInputTypeDefinition);
            if (typeInputTypeDef != null) CustomConfigTypes = new InputTypes(typeInputTypeDef).CustomConfigTypes;
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsObsolete { get; set; }

        /// <summary>
        /// WIP 12.10 Deprecation status of the input type - would always contain a message if it's deprecated
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ObsoleteMessage { get; set; }

        /// <summary>
        /// WIP 12.10 Recommendation status
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsRecommended { get; }

        /// <summary>
        /// WIP 12.10 Default selection status
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsDefault { get; }

        #endregion

        #region New v16.08 / experimental

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string CustomConfigTypes { get; }

        public IDictionary<string, bool> ConfigTypes(ILog log = null)
        {
            if (_configTypes != null) return _configTypes;

            var l = log.Fn<IDictionary<string, bool>>();

            var newDic = new Dictionary<string, bool>(InvariantCultureIgnoreCase) { [AttributeMetadata.TypeGeneral] = true };
            
            // New setup v16.08
            // If we have custom settings, take @All and the custom settings only
            if (CustomConfigTypes.HasValue())
            {
                try
                {
                    var parts = CustomConfigTypes
                        .Split(',')
                        .Select(s => s.Trim().TrimStart('@'))
                        .Where(s => s.HasValue())
                        .ToArray();
                    foreach (var part in parts) newDic[part] = true;
                    return l.Return(newDic, $"custom list {newDic.Count}");
                }
                catch (Exception ex)
                {
                    // Just log and fall back to default
                    l.Ex(ex);
                }
            }

            // Standard setup - this has been the default behavior since ca. v6
            // @All, @MainType, @current-Type
            // eg. [@All, @String, @string-url-path]
            try
            {
                var mainType = Type.Split('-')[0];
                mainType = mainType[0].ToString().ToUpper() + mainType.Substring(1);
                newDic["@" + mainType] = true;
                newDic["@" + Type] = true;
            }
            catch (Exception ex)
            {
                // Just log and fallback to the almost empty dictionary
                l.Ex(ex);
            }

            return l.Return(_configTypes = newDic, $"{newDic.Count}");
        }
        private IDictionary<string, bool> _configTypes;

        #endregion

    }
}
