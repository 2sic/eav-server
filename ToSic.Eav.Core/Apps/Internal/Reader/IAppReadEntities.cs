using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps;

public interface IAppReadEntities
{
    IImmutableList<IEntity> List { get; }

    IEntity? GetDraft(IEntity entity);

    IEntity? GetPublished(IEntity entity);

}