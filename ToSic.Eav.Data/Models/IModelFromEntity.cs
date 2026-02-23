namespace ToSic.Eav.Models;

/// <summary>
/// Marks classes and interfaces as being able to handel Entities as the underlying data.
/// </summary>
/// <remarks>
/// The sole purpose of this is to ensure that classes/records/interfaces can declare that they can be used as models of entities.
/// This should help the compiler detect early on, if interfaces / objects passed to As{TModel} are valid or not.
/// It does not have any properties, since that would put a burden on anything that 
/// 
/// History
///
/// * WIP v21
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice("may change or rename at any time")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IModelFromEntity: IModelFromData;