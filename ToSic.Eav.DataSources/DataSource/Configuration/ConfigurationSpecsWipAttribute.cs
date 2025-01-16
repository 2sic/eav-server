namespace ToSic.Eav.DataSource;

/// <summary>
/// </summary>
/// <remarks>
/// New in v19.01 WIP
/// </remarks>
[PrivateApi]
[AttributeUsage(validOn: AttributeTargets.Class)]
public class ConfigurationSpecsWipAttribute : Attribute
{
    /// <summary>
    /// Default, empty constructor.
    /// All properties must be added in a named fashion to ensure long-term API consistency.
    /// </summary>
    public ConfigurationSpecsWipAttribute() { }

    public Type SpecsType { get; set; }

}