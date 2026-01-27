namespace ToSic.Eav.Model;

/// <summary>
/// BETA / WIP: Attribute to decorate interfaces to specify a concrete type when creating the model.
/// </summary>
/// <example>
/// ```c#
/// [ModelCreation(Use = typeof(PersonModel))]
/// interface IPersonModel : ICanWrapData
/// {
///   public string Name { get; }
/// }
/// ```
///
/// History:
/// 
/// * New / WIP in v19.01 (internal)
/// * Moved from Sxc.Data.Models to Eav.Model v21.01
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public sealed class ModelCreationAttribute: Attribute
{
    /// <summary>
    /// The type to use when creating a model of this interface.
    /// </summary>
    /// <remarks>
    /// It **must** match (implement or inherit) the type which is being decorated.
    /// Otherwise, it will throw an exception.
    /// </remarks>
    public Type? Use { get; init; }
}
