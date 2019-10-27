using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Apps.Interfaces
{
    [PublicApi.PublicApi]
    public interface IApp : IAppIdentity
    {
        /// <summary>
        /// App Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// App Folder
        /// </summary>
        string Folder { get; }

        /// <summary>
        /// If the app should be hidden from the normal app-picker.
        /// Important to configure apps once and then hide from normal users. 
        /// </summary>
        bool Hidden { get; }

        /// <summary>
        /// GUID of the App.
        /// </summary>
        string AppGuid { get; }

        /// <summary>
        /// Data of the app
        /// </summary>
        IAppData Data { get; }

        /// <summary>
        /// All queries of the app, to access like App.Query["name"]
        /// </summary>
        IDictionary<string, IDataSource> Query { get; }

        /// <summary>
        /// The app metadata - like settings, resources etc.
        /// </summary>
        IMetadataOfItem Metadata { get; }
    }
}
