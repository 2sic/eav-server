namespace ToSic.Eav.Data;

[PrivateApi("Still WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityDecorator12<T>(IEntity baseEntity, T decorator) : EntityWrapper(baseEntity, decorator)
    where T : IDecorator<IEntity>
{
    protected T Decorator = decorator;
}