namespace ToSic.Eav.Models;

// BETA: EXPERIMENTAL WRAPPING OF IEntity for EAV USE CASES
// not for public use ATM, should later be merged with ICanWrap<TSource> in SXC

/// <summary>
/// Marks objects such as custom items or data models, which can receive a _specific_ data-type (entity _or_ typed item) and wrap it.
/// </summary>
/// <remarks>
/// This is more specific than the <see cref="ICanWrapData"/>, since that is just a marker interface.
/// This one specifies that the object has the necessary `Setup()` method to receive the data of the expected type.
/// 
/// Typical use is for custom data such as classes inheriting from [](xref:Custom.Data.CustomItem)
/// which takes an entity and then provides a strongly typed wrapper around it.
/// 
/// History
/// 
/// * Introduced in v21
/// * The `Setup()` method is still internal, as the signature may still change
/// </remarks>
/// <typeparam name="TSource">
/// The data type which can be accepted.
/// Must be <see cref="IEntity"/> or <see cref="ITypedItem"/> (other types not supported for now).
/// </typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice("may change or rename at any time")]
public interface IModelSetup<in TSource>
{
    /// <summary>
    /// Add the contents to use for the wrapper.
    /// We are not doing this in the constructor,
    /// because the object needs to have an empty or DI-compatible constructor. 
    /// </summary>
    /// <returns>
    /// `true` if all is ok, `false` if not.
    /// A typical example would be a wrapper which is setup with `null` and it cannot work that way.
    /// On the other hand, if a wrapper is ok with a null source, it would return `true`.
    /// </returns>
    public bool SetupModel(TSource? source);
}