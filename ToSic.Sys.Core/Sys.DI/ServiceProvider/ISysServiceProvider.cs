namespace ToSic.Sys.DI;

/// <summary>
/// Special interface to clearly see when our service provider - which can handle stacking, is being used.
/// </summary>
/// <remarks>
/// Very experimental!!!
/// </remarks>
public interface ISysServiceProvider : ISystemServiceProviderExplicit;