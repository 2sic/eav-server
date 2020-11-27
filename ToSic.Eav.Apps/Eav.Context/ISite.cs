using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    /// <summary>
    /// Describes a tenant - this is what the Environment calls a tenant (like a portal in DNN)
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface ISite: IWebResource, IZoneIdentity, IGetDefaultLanguage
    {
        #region Constructor Helper

        /// <summary>
        /// This is a special constructor where the tenant object is re-initialized with a specific tenant id
        /// </summary>
        /// <returns></returns>
        ISite Init(int siteId);

        #endregion

        /// <summary>
        /// The tenant name for human readability (UIs)
        /// Usually the DNN PortalName
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The root path of the tenant for accessing files using server code
        /// </summary>
        [PrivateApi] string AppsRootPhysical { get; }

        [PrivateApi] string AppsRootPhysicalFull { get; }

        [PrivateApi] string AppsRootLink { get; }

        [PrivateApi]
        string ContentPath { get; }
    }
}
