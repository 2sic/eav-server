using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    /// <summary>
    /// Describes a tenant - this is what the Environment calls a tenant (like a portal in DNN)
    /// </summary>
    [PrivateApi]
    public interface ISite: IZoneIdentity, IGetDefaultLanguage
    {
        #region Constructor Helper

        /// <summary>
        /// This is a special constructor where the tenant object is re-initialized with a specific tenant id
        /// </summary>
        /// <returns></returns>
        ISite Init(int siteId);

        #endregion

        /// <summary>
        /// The Id of the site in systems like DNN and Oqtane.
        /// In DNN this is the same as the PortalId
        /// </summary>
        int Id { get; }

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


        // unsure about this
        /// <summary>
        /// The resource specific url, like the one to this page or portal
        /// </summary>
        string Url { get; }

    }
}
