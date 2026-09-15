namespace ToSic.Sys.Services;

// Note: we can't call it `Dependencies` otherwise all inheriting dependencies would have to use the full namespace.

/// <summary>
/// Base class for all Dependency helpers on services.
/// </summary>
/// <remarks>
/// These are helper objects to get dependencies for a class.
/// It should be used when the owning-class is expected to be inherited.
/// This is important for _inheriting_ classes to keep a stable constructor.
///
/// Obsolete logging-connection signatures remain as no-ops for compatibility.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record DependenciesBase : IDependencies
{
    // ReSharper disable once UnusedParameter.Local
    protected DependenciesBase(NoParamOrder npo = default, object[]? connect = default)
    { }


    /// <summary>
    /// <summary>
    /// Obsolete compatibility method; ignored.
    /// </summary>
    /// <param name="services">Former logging dependencies; ignored.</param>
    protected void ConnectLogs(object[] services)
    { }

    void ILazyInitLog.SetLog(ILog? parentLog)
    { }
}
