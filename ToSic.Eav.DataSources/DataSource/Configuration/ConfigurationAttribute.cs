namespace ToSic.Eav.DataSource;

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
/// <remarks>
/// New in v16.
/// </remarks>
[PublicApi]
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
    /// You should normally not use this
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

    /// <summary>
    /// Optional field name to use, if you must override the default.
    /// The default is that it uses the same name as the property, and this is highly recommended.
    /// You should only set the field, if you are renaming the property and it must still get data from old configurations.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// The fallback value to use, if code/configuration don't give this another value.
    /// Note that internally it will be converted to a string, because Tokens work that way. 
    /// </summary>
    public object Fallback { get; set; }

    /// <summary>
    /// Determine if the configuration is cache relevant.
    /// It usually is.
    /// But anything that doesn't change the output should be set to false, to avoid cache bloat.
    /// </summary>
    public bool CacheRelevant { get; set; } = true;
}