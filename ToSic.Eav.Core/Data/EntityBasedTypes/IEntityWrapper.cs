namespace ToSic.Eav.Data;

/// <summary>
/// A interface to ensure all things that carry an IEntity can be compared based on the Entity they carry.
/// </summary>
[PrivateApi("Hide, was public before 2023-08-10")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IEntityWrapper: IHasDecorators<IEntity>, IMultiWrapper<IEntity>, ICanBeEntity
{
    /// <summary>
    /// The underlying entity. 
    /// </summary>
    /// <returns>The entity, or null if not provided</returns>
    new IEntity Entity { get; }
}