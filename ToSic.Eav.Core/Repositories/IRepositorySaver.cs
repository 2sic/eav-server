using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Interfaces
{
    public interface IRepositorySaver
    {
        void SaveContentType(IContentType type);
    }
}
