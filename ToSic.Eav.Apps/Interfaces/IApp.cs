using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// An App in memory - for quickly getting things done with the app data, queries etc.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IApp : IAppIdentityWithPublishingState, IHasMetadata
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
        /// GUID of the App as string.
        /// </summary>
        /// <returns>The internal GUID of the app.</returns>
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

        [PrivateApi]
        AppState AppState { get; }

        #endregion
    }
}
