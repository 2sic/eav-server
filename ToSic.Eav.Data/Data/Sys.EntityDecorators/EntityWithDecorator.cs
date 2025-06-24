using ToSic.Eav.Data.Entities.Sys.Wrappers;

namespace ToSic.Eav.Data.EntityDecorators.Sys;

[PrivateApi("Still WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityWithDecorator<TDecorator>(IEntity baseEntity, TDecorator decorator)
    : EntityWrapper(baseEntity, decorator)
    where TDecorator : IDecorator<IEntity>;