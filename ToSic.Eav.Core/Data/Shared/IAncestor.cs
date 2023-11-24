using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Shared;

public interface IAncestor: IAppIdentity, IDecorator
{
    int Id { get; set;  }
}