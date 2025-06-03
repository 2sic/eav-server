namespace ToSic.Eav.Data;

[PrivateApi("Still WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityWithDecorator<TDecorator>(IEntity baseEntity, TDecorator decorator)
    : EntityWrapper(baseEntity, decorator)
    where TDecorator : IDecorator<IEntity>;