using ToSic.Eav.Data;

namespace ToSic.Eav.Repositories;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRepositorySaver
{
    void SaveContentType(IContentType type);
}