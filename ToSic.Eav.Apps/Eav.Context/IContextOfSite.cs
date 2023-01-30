using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    public interface IContextOfSite: IHasLog
    {
        ///// <summary>
        ///// ContextOfSiteDependencies handed around, so it's part of the context being passed around
        ///// </summary>
        //[PrivateApi]
        //ContextOfSite.ContextOfSiteDependencies SiteDeps { get; }

        /// <summary>
        /// The website the current request is running in
        /// </summary>
        ISite Site { get; set; }

        /// <summary>
        /// The user in the current request / context
        /// </summary>
        IUser User { get; }

        /// <summary>
        /// Determines if the user is regarded as an editor within this context.
        /// Will vary depending on how much we know about the user. In the Site-context it only depends
        /// on user permissions.
        /// </summary>
        bool UserMayEdit { get; }


        /// <summary>
        /// Create a clone of the context, usually for then making a slightly different context
        /// </summary>
        /// <returns></returns>
        IContextOfSite Clone(ILog newParentLog);

    }
}
