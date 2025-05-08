using ToSic.Lib.Data;

namespace ToSic.Eav.Context;

public interface IUser<out T>: IUser, IWrapper<T>;