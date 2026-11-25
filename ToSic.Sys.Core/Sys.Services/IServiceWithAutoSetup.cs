namespace ToSic.Sys.Services;

/// <summary>
/// WIP Experimental 2dm 2026-11-14.
/// Trying to get services which use AppContext to support injected options.
/// </summary>
/// <typeparam name="TOptions"></typeparam>
[PrivateApi]
public interface IServiceWithAutoSetup<in TOptions> : IServiceWithSetup<TOptions>
{
    void AutoSetup(Func<TOptions> options);
}