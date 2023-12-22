using ToSic.Eav.Data;

namespace ToSic.Eav.Repositories;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRepositorySaver
{
    void SaveContentType(IContentType type);
}