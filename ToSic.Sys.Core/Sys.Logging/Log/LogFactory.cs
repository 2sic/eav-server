using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ToSic.Sys.Logging;

/// <summary>
/// Application-wide log factory access.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogFactory
{
    private static readonly LogFactorySelector Selector = new(LegacyLogFactory.Instance);
    private static readonly BootstrapMelLogging BootstrapMel = new();

    /// <summary>Selects the configured logging stack before host DI exists. Repeating the same selection is safe.</summary>
    public static ILogFactory Initialize(bool useMel)
    {
        var factory = useMel ? BootstrapMel.Factory : LegacyLogFactory.Instance;
        Selector.Select(factory);
        return factory;
    }

    /// <summary>Connects the selected MEL stack to the host-owned logger factory without selecting another mode.</summary>
    public static void Bind(IServiceProvider services)
    {
        if (!ReferenceEquals(services.GetRequiredService<ILogFactory>(), Selector.Current))
            throw new InvalidOperationException("DI logging factory differs from the selected factory.");
        if (ReferenceEquals(Selector.Current, BootstrapMel.Factory))
        {
            var dropped = BootstrapMel.Bind(services.GetRequiredService<ILoggerFactory>());
            if (dropped > 0)
                BootLog.Log.W($"Boot logging buffer dropped {dropped} events before MEL was ready.");
        }
    }

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
