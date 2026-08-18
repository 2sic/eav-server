using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// This is a pre-registered service which will throw an exception if it is requested in a place where it was not previously injected.
/// </summary>
public class AppWorkCtxForDiMissing: IAppWorkCtx, IAppWorkCtxForDiWip
{
    public AppWorkCtxForDiMissing()
    {
         // throw new ArgumentNullException(ErrorMessage);
    }

    private const string ErrorMessage = $"A service seems to request {nameof(IAppWorkCtx)} but in a place where it was not previously injected.";

    public int ZoneId => throw new ArgumentNullException(ErrorMessage);
    public int AppId => throw new ArgumentNullException(ErrorMessage);
    public IAppReader AppReader => throw new ArgumentNullException(ErrorMessage);
    public DbStorage DbStorage => throw new ArgumentNullException(ErrorMessage);
}