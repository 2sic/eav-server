using ToSic.Eav.Data;

namespace ToSic.Eav.Interfaces;

public interface IRepositorySaver
{
    void SaveContentType(IContentType type);
}