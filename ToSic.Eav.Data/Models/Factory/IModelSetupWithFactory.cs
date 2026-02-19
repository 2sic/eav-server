namespace ToSic.Eav.Models.Factory;

/// <summary>
/// WIP - nothing done yet, just as a reminder.
/// Goal is that some wrappers require a factory, and these should be marked as such,
/// so that a simple wrapper helper can detect and warn about this.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IModelFactoryRequired;

/// <summary>
/// Marks objects such as custom items or data models, which can receive a _specific_ data-type (entity _or_ typed item) and wrap it.
/// </summary>
/// <remarks>
/// This is more specific than the <see cref="IModelFromData"/>, since that is just a marker interface.
/// This one specifies that the object has the necessary `Setup()` method to receive the data of the expected type.
/// 
/// Typical use is for custom data such as classes inheriting from [](xref:Custom.Data.CustomItem)
/// which takes an entity and then provides a strongly typed wrapper around it.
/// 
/// History
/// 
/// * Introduced in v17.02 under a slightly different name
/// * Made visible in the docs for better understanding in v19.01
/// * The `Setup()` method is still internal, as the signature may still change
/// * Renamed from `ToSic.Sxc.Data.ICanWrap{TSource}` to `ToSic.Eav.Models.Factory.IModelSetupWithFactory{TSource}` in v21.01
/// </remarks>
/// <typeparam name="TSource">
/// The data type which can be accepted.
/// Must be <see cref="IEntity"/> or <see cref="ITypedItem"/> (other types not supported for now).
/// </typeparam>
public interface IModelSetupWithFactory<in TSource> : IModelFactoryRequired, IModelFromData
{
    /// <summary>
    /// Add the data to use for the wrapper.
    /// We are not doing this in the constructor,
    /// because the object needs to have an empty or DI-compatible constructor. 
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public void Setup(TSource source, IModelFactory modelFactory);

}