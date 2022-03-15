using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        /// <inheritdoc />
        [PrivateApi]
        public ISite Site { get; protected set; }


        #region Paths

        [PrivateApi]
        public string PhysicalPath => AppState.GetPiggyBack(nameof(PhysicalPath), () => Path.Combine(Site.AppsRootPhysicalFull, Folder));

        #endregion

    }
}
