
namespace ToSic.Eav.Models;

/// <summary>
/// Mark custom models/interfaces for additional specifications.
/// </summary>
/// <remarks>
/// This marks custom models to enable checks and more automation, such as:
///
/// * Specify an alternate content type name than the default, which would have to match the class/interface name
/// * Ensure that the model is only used for specific content-type(s) which don't match the model name
/// * Allow the model to be used with all content types `*`
/// * Automatically find the best stream of data to use with the model, if it doesn't match the model name
/// 
/// Typical use is for custom data such as classes inheriting from [](xref:Custom.Data.CustomItem)
/// which takes an entity and then provides a strongly typed wrapper around it.
/// 
/// History:
/// 
/// * New / WIP in v19.01 (internal)
/// * Moved from Sxc.Data.Models to Eav.Models v21.01
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice("may change or rename at any time")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class ModelSpecsAttribute: Attribute
{
    /// <summary>
    /// Determines which content-type names are expected when converting to this model.
    /// </summary>
    /// <remarks>
    /// Usually this is checked when converting Entities to the model.
    /// If it doesn't match, will then throw an error, unless you specify `skipTypeCheck: true` in the conversion method.
    /// 
    /// Typically just one value, such as "Article" or "Product".
    /// But it will also support "*" for anything, or (future!) a comma-separated list of content-type names.
    ///
    /// Note: This is not required.
    /// Without this value, the class name will be used to determine if a conversion is valid.
    /// 
    /// History: WIP 19.01 / v21.01
    /// </remarks>
    public string? ContentType
    {
        get;
#if NETCOREAPP
        init;
#else
        set;
#endif
    }

    /// <summary>
    /// WIP, not officially supported yet.
    /// </summary>
    public string? Stream
    {
        get;
#if NETCOREAPP
        init;
#else
        set;
#endif
    }

    // TODO: MAKE INTERNAL AGAIN AFTER MOVING TO ToSic.Sxc.Custom
    [PrivateApi]
    public const string ForAnyContentType = "*";

    /// <summary>
    /// The type to use when creating a model of this interface.
    /// </summary>
    /// <remarks>
    /// It **must** match (implement or inherit) the type which is being decorated.
    /// Otherwise, it will throw an exception.
    /// </remarks>
    public Type? Use
    {
        get;
#if NETCOREAPP
        init;
#else
        set;
#endif
    }

}