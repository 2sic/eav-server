using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Ancestors.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAncestor: IAppIdentity, IDecorator
{
    int Id { get; set;  }
}