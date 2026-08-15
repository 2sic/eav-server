namespace ToSic.Sys.DI;

public interface ICanInitialize<TService>
{
    internal Action<TService>? _initCall { get; set; }
}

public static class ICanInitializeExtensions
{
    /// <summary>
    /// Set the init-command as needed
    /// </summary>
    /// <param name="canInitialize"></param>
    /// <param name="newInitCall"></param>
    /// <param name="allowReplace">Allow replacing the set-init</param>
    public static TFactory SetInit<TFactory, TService>(this TFactory canInitialize, Action<TService> newInitCall, bool allowReplace = false)
        where TFactory: ICanInitialize<TService>
    {
#if DEBUG
        // Warn if we're accidentally replacing init-call, but only do this on debug
        // In most cases it has no consequences, but we should write code that avoids this
        if (canInitialize._initCall != null && !allowReplace)
            throw new($"You tried to call {nameof(SetInit)} twice. This should never happen");
#endif
        canInitialize._initCall = newInitCall;
        return canInitialize;
    }

}