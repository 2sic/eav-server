namespace ToSic.Sys.Services;

/// <summary>
/// Base class for all Dependency helpers on services.
/// </summary>
/// <remarks>
/// These are helper objects to get dependencies for a class.
/// It should be used when the owning-class is expected to be inherited.
/// This is important for _inheriting_ classes to keep a stable constructor.
///
/// Can collect all objects which need the log and init that.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record DependenciesRecord : IDependencies
{
    // ReSharper disable once UnusedParameter.Local
    protected DependenciesRecord(NoParamOrder npo = default, object[]? connect = default)
    {
        if (connect == null)
            return;
        ConnectLogs(connect);
    }


    /// <summary>
    /// Special helper to keep track of all dependencies which need a log, to init once SetLog is called
    /// </summary>
    internal DependenciesLogHelper LogHelper { get; } = new();

    /// <summary>
    /// Add objects to various queues to be auto-initialized when <see cref="DependenciesExtensions.ConnectServices{TDependencies}"/> is called later on
    /// </summary>
    /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
    protected void ConnectLogs(object[] services)
        => LogHelper.Add(services);

    internal bool InitDone;

    void ILazyInitLog.SetLog(ILog? parentLog)
    {
        if (InitDone)
            return;
        LogHelper.SetLog(parentLog);
        InitDone = true;
    }
}
