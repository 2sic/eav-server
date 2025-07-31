using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Apps.Sys.State;

partial class AppState: IEntitiesSource
{
    IEnumerable<IEntity> IEntitiesSource.List => Entities.ImmutableList;

    [field: AllowNull, MaybeNull]
    internal AppStateEntities Entities => field ??= new(this);

    [field: AllowNull, MaybeNull]
    internal AppStatePublishing Publishing => field ??= new(this);
}