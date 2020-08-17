using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Run
{
    /// <summary>
    /// A web resources used in the context of running an app instances.
    /// Usually a tenant/portal, page etc.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IWebResource
    {
        /// <summary>
        /// The Id of the resources, usually the pageId, portalId etc.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The resource specific url, like the one to this page or portal
        /// </summary>
        string Url { get; }
    }
}
