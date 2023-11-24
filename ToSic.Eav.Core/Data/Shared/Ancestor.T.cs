using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Shared;

public class Ancestor<T>: AppIdentity, IAncestor, IDecorator<T>
{
    public Ancestor(IAppIdentity parent, int id) : base(parent)
    {
        Id = id;
    }

    public int Id { get; set;  }
}