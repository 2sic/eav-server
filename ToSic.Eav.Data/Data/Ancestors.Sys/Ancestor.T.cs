using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Ancestors.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class Ancestor<T>(IAppIdentity parent, int id) : AppIdentity(parent), IAncestor, IDecorator<T>
{
    public int Id { get; set;  } = id;
}