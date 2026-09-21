namespace ToSic.Sys.Logging;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class LegacyLogFactory : ILogFactory
{
    public static LegacyLogFactory Instance { get; } = new();

    private LegacyLogFactory() { }

    public ILog Create(string name, ILog? parent, CodeRef code, string? initialMessage = default)
        => new Log(name, parent, code, initialMessage);
}
