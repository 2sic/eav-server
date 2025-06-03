using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps;

public interface IAppReadEntities
{
    /// <summary>
    /// All the entities, including drafts.
    /// </summary>
    /// <remarks>
    /// To only get drafts or only published, use the appropriate extension methods.
    /// * GetListPublished()
    /// * GetListNotHavingDrafts()
    /// </remarks>
    IImmutableList<IEntity> List { get; }

    IEntity? GetDraft(IEntity entity);

    IEntity? GetPublished(IEntity entity);

}