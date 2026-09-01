using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.DI;

/// <summary>
/// Dependency Injection helper to override a service type with a different implementation.
/// </summary>
/// <remarks>
/// When `using (...)` this, it will ensure that any code within the `using` will receive the overridden implementation, while code outside will receive the original implementation.
///
/// This will only have an effect, if a service was initially registered for overriding, using the various <see cref="Register{T}" /> methods.
/// </remarks>
/// <typeparam name="TService"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static partial class OverrideService<TService> where TService : class
{
    /// <summary>
    /// Special container to hold the replaced factory function for the current async context.
    /// </summary>
    private static readonly AsyncLocal<Frame?> CurrentFrame = new();

    /// <summary>
    /// Indicates whether an override is active in the current execution context.
    /// </summary>
    public static bool IsOverridden => CurrentFrame.Value != null;

    /// <summary>
    /// Gets the total number of nested overrides currently active (0 if none).
    /// </summary>
    public static int OverrideCount => CurrentFrame.Value?.Depth ?? 0;

    public static string OverridePath => string.Join(" > ", GetFrames()
        .Select(f => $"{f.CallerName ?? "Unknown"}{(f.Hint != null ? $" ({f.Hint})" : "")}")
        .Reverse());

    // Reusable generator that yields frames from current (leaf) up to top (root)
    private static IEnumerable<Frame> GetFrames()
    {
        for (var f = CurrentFrame.Value; f != null; f = f.Parent)
            yield return f;
    }
    
    /// <summary>
    /// Gets the current active factory delegate.
    /// </summary>
    public static Func<IServiceProvider, TService>? CurrentFactory => CurrentFrame.Value?.Factory;

    /// <summary>
    /// Use Overload 1: Supply a type (preserves transient lifetime via DI container)
    /// </summary>
    /// <typeparam name="TImplementation">The type to resolve instead, must be registered with the DI container.</typeparam>
    /// <param name="hint">An optional hint for debugging purposes.</param>
    /// <param name="cName">The caller member name (automatic)</param>
    public static IDisposable Use<TImplementation>(string? hint = null, [CallerMemberName] string? cName = null)
        where TImplementation : class, TService
    {
        return Use(sp => sp.GetRequiredService<TImplementation>(), hint: hint, cName: cName);
    }

    /// <summary>
    /// Use Overload 2: Supply a specific instance (singleton)
    /// </summary>
    /// <typeparam name="TImplementation">The type of the instance to use.</typeparam>
    /// <param name="value">The specific instance to use.</param>
    /// <param name="hint">An optional hint for debugging purposes.</param>
    /// <param name="cName">The caller member name (automatic)</param>
    public static IDisposable Use<TImplementation>(TImplementation value, string? hint = null, [CallerMemberName] string? cName = null)
        where TImplementation : class, TService
    {
        return Use(_ => value, hint: hint, cName: cName);
    }

    /// <summary>
    /// Use Overload 3: Supply a custom factory function
    /// </summary>
    /// <param name="factory">The factory function to use.</param>
    /// <param name="hint">An optional hint for debugging purposes.</param>
    /// <param name="cName">The caller member name (automatic)</param>
    public static IDisposable Use(Func<IServiceProvider, TService> factory, string? hint = null, [CallerMemberName] string? cName = null)
    {
        var parentFrame = CurrentFrame.Value;

        // Push a new Frame onto the stack
        CurrentFrame.Value = new(factory, OverrideCount + 1, parentFrame, hint, cName);

        return new Scope(parentFrame);
    }

    /// <summary>
    /// Idempotent Begin: Only applies if no override exists
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="cName">The caller member name (automatic)</param>
    /// <returns></returns>
    public static IDisposable UseIfNotOverridden<TImplementation>(string? hint = null, [CallerMemberName] string? cName = null)
        where TImplementation : class, TService
        => IsOverridden
            ? NoOpScope.Instance
            : Use<TImplementation>(hint: hint, cName: cName);

    public static IDisposable UseIfNotOverridden(Func<IServiceProvider, TService> factory, string? hint = null, [CallerMemberName] string? cName = null)
        => IsOverridden
            ? NoOpScope.Instance
            : Use(factory, hint: hint, cName: cName);

    /// <summary>
    /// Private Frame Node (Immutable linked-list node)
    /// </summary>
    /// <param name="Factory"></param>
    /// <param name="Depth"></param>
    /// <param name="Parent"></param>
    /// <param name="CallerName"></param>
    private sealed record Frame(Func<IServiceProvider, TService> Factory, int Depth, Frame? Parent, string? Hint, string? CallerName);

    /// <summary>
    /// Private Scope Handle
    /// </summary>
    /// <param name="previousFrame"></param>
    private sealed class Scope(Frame? previousFrame) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            // Pop the Frame off the stack
            CurrentFrame.Value = previousFrame;
            _disposed = true;
        }
    }

    /// <summary>
    /// Dummy disposable that does nothing, used for idempotent overrides.
    /// </summary>
    private sealed class NoOpScope : IDisposable
    {
        public static readonly NoOpScope Instance = new();
        public void Dispose() { }
    }
}