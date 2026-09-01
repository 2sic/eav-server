namespace ToSic.Sys.Users;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRole<out T>: IRole, IWrapper<T>;