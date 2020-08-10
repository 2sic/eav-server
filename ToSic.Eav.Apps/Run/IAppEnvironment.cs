using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    /// <inheritdoc cref="IEnvironment" />
    /// <summary>
    /// Any object implementing this can provide more information which are app/zone specific. 
    /// </summary>
    [PrivateApi]
    public interface IAppEnvironment : IEnvironment, IHasLog<IAppEnvironment>
    {
        /// <summary>
        /// The environment zone-mapper, which finds the zone-ID for the current tenant (portal) of the environment
        /// </summary>
        IZoneMapper ZoneMapper { get; }

        /// <summary>
        /// Page publishing information of the environment, to detect what the environment 
        /// expects of page-publishing
        /// </summary>
        IPagePublishing PagePublishing { get; }

        ///// <summary>
        ///// Path resolver to get the full path of a file (template, icon etc.) inside an app-folder
        ///// </summary>
        ///// <param name="virtualPath"></param>
        ///// <returns></returns>
        //string MapPath(string virtualPath);
    }
}
