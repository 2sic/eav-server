namespace ToSic.Sys.Users;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IUser<out T>: IUser, IWrapper<T>;