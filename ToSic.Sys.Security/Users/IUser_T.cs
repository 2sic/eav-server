using ToSic.Lib.Data;
using ToSic.Lib.Wrappers;

namespace ToSic.Sys.Users;

public interface IUser<out T>: IUser, IWrapper<T>;