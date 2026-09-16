namespace ToSic.Sys.Services;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class DependenciesExtensions
{
    /// <summary>
    /// Obsolete no-op retained for command-chaining compatibility.
    /// </summary>
    public static TMyServices ConnectServices<TMyServices>(this TMyServices parent, ILog log)
        where TMyServices : IDependencies
        => parent;
}
