namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Base class for all app-work helpers.
/// </summary>
/// <typeparam name="TContext"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class WorkUnitBase<TContext>(string logName) : ServiceBase(logName)
    where TContext : class, IAppWorkCtx
{
    /// <summary>
    /// The current work context
    /// </summary>
    public TContext AppWorkCtx
    {
        get => _appWorkCtx ?? throw new($"Can't use this before {nameof(AppWorkCtx)} is set - pls use a Work-Generator.");
        private set => _appWorkCtx = value;
    }
    private TContext _appWorkCtx;

    /// <summary>
    /// Internal init, should only ever be called by the GenWork... generators
    /// </summary>
    /// <param name="appWorkCtx"></param>
    internal void _initCtx(TContext appWorkCtx) => AppWorkCtx = appWorkCtx;
}