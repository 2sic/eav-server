using System.Runtime.CompilerServices;

namespace ToSic.Sys.Logging;

/// <summary>
/// Application-wide log factory access.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogFactory
{
    private static readonly LogFactorySelector Selector = new(LegacyLogFactory.Instance);

    /// <summary>
    /// Selects the one log factory for this application lifetime.
    /// </summary>
    public static void Select(ILogFactory factory) => Selector.Select(factory);

    /// <summary>
    /// Creates a log using its parent's implementation when available.
    /// </summary>
    public static ILog Create(string name,
        ILog? parent = default,
        string? initialMessage = default,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default)
        => Selector.For(parent).Create(name, parent.GetRealLog(), CodeRef.Create(cPath!, cName!, cLine), initialMessage);
}
