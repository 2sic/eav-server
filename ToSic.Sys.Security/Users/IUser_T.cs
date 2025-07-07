namespace ToSic.Sys.Users;

public interface IUser<out T>: IUser, IWrapper<T>;