using System;
using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// An App in memory - for quickly getting things done with the app data, queries etc.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IApp : IAppIdentity, /*IAppIdentityWithPublishingState,*/ IHasMetadata
    {
        /// <summary>
        /// App Name
        /// </summary>
        /// <returns>The name as configured in the app configuration.</returns>
        string Name { get; }

        /// <summary>
        /// App Folder
        /// </summary>
        /// <returns>The folder as configured in the app configuration.</returns>
        string Folder { get; }

        /// <summary>
        /// If the app should be hidden from the normal app-picker.
        /// Important to configure apps once and then hide from normal users. 
        /// </summary>
        /// <returns>The hidden-state as configured in the app configuration.</returns>
        bool Hidden { get; }

        /// <summary>
        /// NameId of the App - usually a string-GUID
        /// </summary>
        string NameId { get; }

        [PrivateApi]
        [Obsolete("Don't use any more, use NameId instead, will be removed ca. v14")]
        string AppGuid { get; }

        /// <summary>
        /// Data of the app
        /// </summary>
        IAppData Data { get; }

        /// <summary>
        /// All queries of the app, to access like App.Query["name"]
        /// </summary>
        /// <returns>A dictionary with all queries. Internally the dictionary will not be built unless accessed.</returns>
        IDictionary<string, IQuery> Query { get; }

        /// <summary>
        /// The app metadata - like settings, resources etc.
        /// </summary>
        /// <returns>A metadata provider for the app</returns>
        new IMetadataOf Metadata { get; }


        #region Experimental / new


        /// <summary>
        /// The tenant this app belongs to - for example, a DNN portal
        /// </summary>
        [PrivateApi]
        ISite Site { get; }

        [PrivateApi]
        // TODO: MARK as #Deprecated and log access
        ILookUpEngine ConfigurationProvider { get; }

        [PrivateApi]
        Query GetQuery(string name);

        // Note: was here a long time, marked as public in v14.7
        // Made private again in 15.06 because I want to create an interface IAppState
        /// <summary>
        /// The stored / cached, read-only App State
        /// </summary>
        [PrivateApi]
        AppState AppState { get; }

        #endregion
    }
}
