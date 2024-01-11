using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Internal;

partial class EavApp
{
    /// <inheritdoc />
    [PrivateApi]
    public ISite Site { get; protected set; } = services.Site;


    #region Paths

    [PrivateApi]
    public string PhysicalPath => AppStateInt.GetPiggyBack(nameof(PhysicalPath), () => Path.Combine(Site.AppsRootPhysicalFull, Folder));

    #endregion

}