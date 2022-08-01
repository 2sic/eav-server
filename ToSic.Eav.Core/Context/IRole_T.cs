using ToSic.Eav.Data;

namespace ToSic.Eav.Context
{
    public interface IRole<out T>: IRole, IWrapper<T>
    {

    }
}
