namespace ToSic.Eav.Interfaces
{
    public interface IRepositorySaver
    {
        void SaveContentType(IContentType type);

        void SaveEntity(IEntity item);
    }
}
