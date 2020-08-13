﻿using ToSic.Eav.Apps.Run;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    /// <summary>
    /// A unit / block within the CMS. Contains all necessary identification to pass around. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]

    public interface IContainer
    {
        [PrivateApi("Workaround till we have DI to inject the current container")]
        IContainer Init(int id, ILog parentLog);

        /// <summary>
        /// Block ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Page ID
        /// </summary>
        int PageId { get; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        int TenantId { get; }

        /// <summary>
        /// Determines if this is a the primary App (the content-app) as opposed to any additional app
        /// </summary>
        [PrivateApi("don't think this should be here! also not sure if it's the primary - or the contentApp! reason seems to be that we detect it by the DNN module name")]
        bool IsPrimary { get; }

        /// <summary>
        /// Identifies the content-block which should be shown in this container
        /// </summary>
        [PrivateApi("still experimental")]
        IBlockIdentifier BlockIdentifier { get; }
    }
}
