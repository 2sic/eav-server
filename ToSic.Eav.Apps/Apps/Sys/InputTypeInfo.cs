using System.Text.Json.Serialization;
using ToSic.Eav.Data.Attributes.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Data.InputTypes.Sys;
using ToSic.Eav.Metadata;
using ToSic.Sys.Utils;
using static System.StringComparer;
using static ToSic.Eav.Metadata.KnownDecorators;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Describes an input field type with its labels, abilities etc. 
/// This is so that input fields can self-describe.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class InputTypeInfo(string type, string label, string description, string assets, bool disableI18N, string ngAssets, bool useAdam, string source)
{
    public InputTypeInfo(string type, string label, string description, string assets, bool disableI18N, string ngAssets, bool useAdam, string source, IMetadataOf metadata = null)
        : this(type, label, description, assets, disableI18N, ngAssets, useAdam, source)
    {
        if (metadata == null) return;

        Metadata = metadata;

        if (metadata.HasType(IsObsoleteDecoratorId))
        {
            IsObsolete = true;
            ObsoleteMessage = metadata.GetBestValue<string>(MessageField, IsObsoleteDecoratorId);
        }
        if (metadata.HasType(RecommendedDecoratorId)) IsRecommended = true;

        if (metadata.HasType(IsDefaultDecorator)) IsDefault = true;

        var typeInputTypeDef = metadata.FirstOrDefaultOfType(InputTypeDefinition.TypeForInputTypeDefinition);
        if (typeInputTypeDef != null) ConfigTypes = new InputTypeDefinition(typeInputTypeDef).ConfigTypes;
    }

    /// <summary>
    /// The type this input is for, like `String`, `string-picker` etc.
    /// </summary>
    public string Type => type;

    public string Label => label;

    public string Description => description;

    public string Assets => assets;

    public IMetadataOf Metadata { get; }

    #region new in 2sxc 10 / eav 5

    // ReSharper disable once InconsistentNaming
    public bool DisableI18n => disableI18N;

    /// <summary>
    /// Additional resources to load (js/css)
    /// </summary>
    public string AngularAssets => ngAssets;

    /// <summary>
    /// Activates ADAM in the UI for this input type
    /// </summary>
    public bool UseAdam => useAdam;

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

    /// <summary>
    /// Just an internal info for better debugging - not meant to be used
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Source => source;

    /// <summary>
    /// Configuration Content-Types used for this input type.
    /// Almost always empty, in which case the logic uses defaults 
    /// </summary>
    /// <remarks>WIP/BETA v16.08</remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ConfigTypes { get; }

    /// <summary>
    /// Create a fresh dictionary with the minimum config-types
    /// all inputs need. 
    /// </summary>
    /// <returns>Dictionary with name/required - ATM all required are set to true</returns>
    public static Dictionary<string, bool> NewDefaultConfigTypesDic()
        => new(InvariantCultureIgnoreCase) { [AttributeMetadataConstants.TypeGeneral] = true };

    /// <summary>
    /// Internal processing to get the config-types in the format we need.
    /// This varies a bit depending on if <see cref="ConfigTypes"/> is provided or empty.
    ///
    /// Note that it caches the result for the lifetime of this object, which can be quite long.
    /// </summary>
    /// <param name="log">Optional log to record what it does</param>
    /// <returns>Dictionary with name/required - ATM all required are set to true</returns>
    public IDictionary<string, bool> ConfigTypesDic(ILog log = null)
    {
        if (_configTypesDic != null) return _configTypesDic;

        var l = log.Fn<IDictionary<string, bool>>();

        var newDic = NewDefaultConfigTypesDic();

        // New setup v16.08
        // If we have custom settings, take @All and the custom settings only
        if (ConfigTypes.HasValue())
            try
            {
                var parts = ConfigTypes.CsvToArrayWithoutEmpty();
                foreach (var part in parts)
                    newDic[part] = true;
                return l.Return(_configTypesDic = newDic, $"custom list {newDic.Count}");
            }
            catch (Exception ex)
            {
                l.Ex(ex);   // Just log and fall back to default
            }

        // Standard setup - this has been the default behavior since ca. v6
        // @All, @MainType, @current-Type
        // like [@All, @String, @string-url-path]
        try
        {
            var mainType = Type.Split('-')[0];
            var mainTypeFinal = Enum.GetNames(typeof(ValueTypes))
                                    .FirstOrDefault(t => t.EqualsInsensitive(mainType))
                                // fallback to just capitalize first letter, should never happen
                                ?? mainType[0].ToString().ToUpper() + mainType.Substring(1);
            newDic["@" + mainTypeFinal] = true;
            newDic["@" + Type] = true;
        }
        catch (Exception ex)
        {
            // Just log and fallback to the almost empty dictionary
            l.Ex(ex);
        }

        return l.Return(_configTypesDic = newDic, $"{newDic.Count}");
    }
    private IDictionary<string, bool> _configTypesDic;

    #endregion

    /// <summary>
    /// For better debugging
    /// </summary>
    public override string ToString() => $"{Type} - {Label}";
}