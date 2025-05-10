// ReSharper disable RedundantAccessorBody
namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Base class for all app-work helpers.
/// </summary>
/// <typeparam name="TContext"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class WorkUnitBase<TContext>(string logName, object[] connect = default) : ServiceBase(logName, connect: connect ?? [])
    where TContext : class, IAppWorkCtx
{
    /// <summary>
    /// The current work context
    /// </summary>
    public TContext AppWorkCtx
    {
        get => field ?? throw new($"Can't use this before {nameof(AppWorkCtx)} is set - pls use a Work-Generator.");
        private set => field = value;
    }

    /// <summary>
    /// Internal init, should only ever be called by the GenWork... generators
    /// </summary>
    /// <param name="appWorkCtx"></param>
    internal void _initCtx(TContext appWorkCtx) => AppWorkCtx = appWorkCtx;
}