using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// An App in memory - for quickly getting things done with the app data, queries etc.
    /// </summary>
    [PublicApi]
    public interface IApp : IAppIdentity
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
        IDictionary<string, IDataSource> Query { get; }

        /// <summary>
        /// The app metadata - like settings, resources etc.
        /// </summary>
        /// <returns>A metadata provider for the app</returns>
        IMetadataOfItem Metadata { get; }


        #region Experimental / new

        /// <summary>
        /// The tenant this app belongs to - for example, a DNN portal
        /// </summary>
        [PrivateApi]
        ITenant Tenant { get; }

        [PrivateApi]
        ITokenListFiller ConfigurationProvider { get; }

        [PrivateApi]
        DeferredQuery GetQuery(string name);

        #endregion
    }
}
