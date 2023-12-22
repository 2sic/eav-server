using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Shared;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Ancestor<T>: AppIdentity, IAncestor, IDecorator<T>
{
    public Ancestor(IAppIdentity parent, int id) : base(parent)
    {
        Id = id;
    }

    public int Id { get; set;  }
}