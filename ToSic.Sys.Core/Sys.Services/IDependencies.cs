namespace ToSic.Sys.Services;

/// <summary>
/// Marks dependency wrapper classes, so that they must implement ILazyInitLog, which is important for logging and lazy initialization of dependencies.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDependencies: ILazyInitLog;
