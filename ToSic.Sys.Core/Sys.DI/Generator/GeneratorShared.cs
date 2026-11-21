namespace ToSic.Sys.DI;

internal static class GeneratorShared
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Throws if the provided options have the wrong type.</exception>
    /// <exception cref="NullReferenceException">Throws if the service is null.</exception>
    internal static TService SetupOrThrow<TService, TOptions>(TOptions options, TService instance)
    {
        if (instance is not IServiceWithSetup<TOptions> settable)
            throw new NotSupportedException($"The service {typeof(TService).Name} doesn't support setting options of type {typeof(TOptions).Name}");
        settable.Setup(options);
        return instance;
    }
}
