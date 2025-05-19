namespace ToSic.Lib.Services;

/// <summary>
/// Marks services which can be setup with options.
/// Usually used together with a Generator{TService, TOptions} to create the service with options.
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public interface IServiceWithSetup<in TOptions>
{
    void Setup(TOptions options);
}
