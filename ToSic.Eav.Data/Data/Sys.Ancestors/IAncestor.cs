using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Sys.Ancestors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAncestor: IAppIdentity, IDecorator
{
    int Id { get; set;  }
}