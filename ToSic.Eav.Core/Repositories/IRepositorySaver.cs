using ToSic.Eav.Data;

namespace ToSic.Eav.Interfaces
{
    public interface IRepositorySaver
    {
        void SaveContentType(IContentType type);

        // 2020-07-31 2dm - never used #2134
        //void SaveEntity(Data.IEntity item);
    }
}
