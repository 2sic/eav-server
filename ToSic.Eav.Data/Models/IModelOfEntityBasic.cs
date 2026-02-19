namespace ToSic.Eav.Models;

/// <summary>
/// Foundation for interfaces which enhance <see cref="ModelOfEntityBasic"/> to always include `Id`, `Guid` and `Title`
/// </summary>
/// <remarks>
/// This is used for more type safety - so you base your interfaces - like IPerson on this,
/// otherwise you're IPerson would be missing the Title, Id, Guid
/// 
/// * Introduced in v21.01
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IModelOfEntityBasic: ICanBeEntity
{
    /// <summary>
    /// The title as string.
    /// </summary>
    /// <remarks>Can be overriden by other parts, if necessary.</remarks>
    /// <returns>The title, or an empty string if not available or not string-able</returns>
    string Title { get; }

    /// <summary>
    /// The entity id, as quick, nice accessor.
    /// </summary>
    /// <returns>The id, or 0 if no entity available</returns>
    int Id { get; }

    /// <summary>
    /// The entity guid, as quick, nice accessor. 
    /// </summary>
    /// <returns>The guid, or an empty-guid of no entity available</returns>
    Guid Guid { get; }
}