using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// This is a pre-registered service which will throw an exception if it is requested in a place where it was not previously injected.
/// </summary>
public class AppWorkContextNotReplaced: IAppWorkContext
{
    public AppWorkContextNotReplaced()
    {
         // throw new ArgumentNullException(ErrorMessage);
    }

    private const string ErrorMessage = $"A service seems to request {nameof(IAppWorkContext)} but in a place where it was not previously injected.";

    public int ZoneId => throw new ArgumentNullException(ErrorMessage);
    public int AppId => throw new ArgumentNullException(ErrorMessage);
    public IAppReader AppReader => throw new ArgumentNullException(ErrorMessage);
    public DbStorage DbStorage => throw new ArgumentNullException(ErrorMessage);
    public IDataSource Data => throw new ArgumentNullException(ErrorMessage);
}