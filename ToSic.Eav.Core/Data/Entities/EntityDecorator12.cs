using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

[PrivateApi("Still WIP")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntityDecorator12<T>: EntityWrapper where T: IDecorator<IEntity>
{
    protected T Decorator;

    public EntityDecorator12(IEntity baseEntity, T decorator) : base(baseEntity, decorator)
    {
        Decorator = decorator;
    }
}