using System.IO;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        [PrivateApi]
        protected IEnvironment Env;
        
        /// <inheritdoc />
        [PrivateApi]
        public ISite Site { get; protected set; }


        #region Paths

        [PrivateApi]
        public string PhysicalPath => Path.Combine(Site.AppsRootPhysicalFull, Folder);

        #endregion

    }
}
