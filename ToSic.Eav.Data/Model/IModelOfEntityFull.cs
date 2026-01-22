using ToSic.Eav.Metadata;

namespace ToSic.Eav.Model;

/// <summary>
/// Foundation for interfaces which will enhance <see cref="ModelOfEntity"/> which gets its data from an Entity. <br/>
/// </summary>
/// <remarks>
/// This is used for more type safety - so you base your interfaces - like IPerson on this,
/// otherwise you're IPerson would be missing the Title, Id, Guid
/// 
/// * Introduced in v21.01
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IModelOfEntityFull: ICanBeEntity, IHasMetadata
{
    /// <inheritdoc cref="ModelOfEntity.Title"/>
    string Title { get; }

    /// <inheritdoc cref="ModelOfEntity.Id"/>
    int Id { get; }

    /// <inheritdoc cref="ModelOfEntity.Guid"/>
    Guid Guid { get; }
}