using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Services;

public interface IAppEntityService
{
    IImmutableList<IEntity> List { get; }

    IEntity GetDraft(IEntity entity);

    IEntity GetPublished(IEntity entity);

}