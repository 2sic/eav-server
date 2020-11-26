using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public interface IRunContextCore
    {
        /// <summary>
        /// The service provider which must often be handed around, so it's part of the context being passed around
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// The website the current request is running in
        /// </summary>
        ISite Site { get; set; }

        /// <summary>
        /// The user in the current request / context
        /// </summary>
        IUser User { get; }

        /// <summary>
        /// Create a clone of the context, usually for then making a slightly different context
        /// </summary>
        /// <returns></returns>
        IRunContextCore Clone();

    }
}
