using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Shared;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAncestor: IAppIdentity, IDecorator
{
    int Id { get; set;  }
}