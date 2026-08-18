namespace ToSic.Sys.DI;

internal static class LazyHelpers
{
    internal static Action<TService> ThrowIfInitAlreadySet<TService>(Action<TService>? oldInit, Action<TService> newInit, bool allowReplace = false)
    {
#if DEBUG
        // Warn if we're accidentally replacing init-call, but only do this on debug
        // In most cases it has no consequences, but we should write code that avoids this
        if (oldInit != null && !allowReplace)
            throw new($"You tried to call {nameof(Generator<>.SetInit)} twice. This should never happen");
#endif
        return newInit;
    }
    
}