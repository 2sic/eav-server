using ToSic.Eav.Data;

namespace ToSic.Eav.Run
{
    public interface IUser<out T>:IUser, IWrapper<T>
    {

    }
}
