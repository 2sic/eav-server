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
        public ISite Site { get; protected set; }


        #region Paths

        //[PrivateApi]
        //protected string GetRootPath() => Path.Combine(Tenant.AppsRootPhysical, Folder);

        [PrivateApi]
        public string PhysicalPath => /*_serverPaths.FullAppPath(*/Path.Combine(Site.AppsRootPhysicalFull, Folder)/*)*/;

        #endregion

    }
}
