using ToSic.Lib.Wrappers;

namespace ToSic.Sys.Users;

public interface IRole<out T>: IRole, IWrapper<T>;