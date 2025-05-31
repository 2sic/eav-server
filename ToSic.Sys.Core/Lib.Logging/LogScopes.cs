namespace ToSic.Lib.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LogScopes
{
    /// <summary>
    /// Most default prefix/scope for library features
    /// </summary>
    public const string Lib = "Lib";

    /// <summary>
    /// Anything using this prefix/scope allows the program to run, but doesn't do anything useful.
    /// Like a security check which always says not-allowed, or an exporter which doesn't export
    /// </summary>
    public const string NotImplemented = "NOT";

    /// <summary>
    /// Anything using this prefix is a base implementation which works, but doesn't
    /// support anything more advanced / specific to a platform. It's usually fallback implementation.
    /// </summary>
    public const string Base = "Bas";

    /// <summary>
    /// Scope / prefix to use when the scope wasn't provided.
    /// </summary>
    public const string Unknown = "tdo";
}