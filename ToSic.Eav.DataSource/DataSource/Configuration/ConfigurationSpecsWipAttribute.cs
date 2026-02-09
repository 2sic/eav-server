//namespace ToSic.Eav.DataSource;

///// <summary>
///// Experimental - not yet in use.
///// 2dm: I believe the idea was to have a "Parameters" object which would be separate from the DataSource,
///// and provide the properties which could be set, making the DS read-only and not expose any properties at all.
/////
///// It is called in `AutoLoadAllConfigMasks` but currently the code is disabled
///// </summary>
///// <remarks>
///// New in v19.01 WIP
///// </remarks>
//[PrivateApi]
//[AttributeUsage(validOn: AttributeTargets.Class)]
//public class ConfigurationSpecsWipAttribute : Attribute
//{
//    /// <summary>
//    /// Default, empty constructor.
//    /// All properties must be added in a named fashion to ensure long-term API consistency.
//    /// </summary>
//    public ConfigurationSpecsWipAttribute() { }

//    public Type? SpecsType { get; set; }

//}