using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Shared;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Ancestor<T>(IAppIdentity parent, int id) : AppIdentity(parent), IAncestor, IDecorator<T>
{
    public int Id { get; set;  } = id;
}