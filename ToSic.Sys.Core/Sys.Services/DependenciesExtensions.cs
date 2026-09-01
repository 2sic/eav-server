namespace ToSic.Sys.Services;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class DependenciesExtensions
{
    /// <summary>
    /// Auto-initialize the log on all dependencies.
    /// Special format to allow command chaining, so it returns itself.
    /// </summary>
    public static TMyServices ConnectServices<TMyServices>(this TMyServices parent, ILog log)
        where TMyServices : IDependencies
    {
        parent.SetLog(log);
        return parent;
    }
}