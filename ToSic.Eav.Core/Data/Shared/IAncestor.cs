using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Shared;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAncestor: IAppIdentity, IDecorator
{
    int Id { get; set;  }
}