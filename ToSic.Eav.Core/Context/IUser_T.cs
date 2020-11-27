using ToSic.Eav.Data;

namespace ToSic.Eav.Context
{
    public interface IUser<out T>: IUser, IWrapper<T>
    {

    }
}
