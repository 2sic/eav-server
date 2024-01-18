using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

[PrivateApi("Still WIP")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntityDecorator12<T>(IEntity baseEntity, T decorator) : EntityWrapper(baseEntity, decorator)
    where T : IDecorator<IEntity>
{
    protected T Decorator = decorator;
}