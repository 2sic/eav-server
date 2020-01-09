using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using IDataSource = ToSic.Eav.DataSources.IDataSource;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// An App-DataSource which also provides direct commands to edit/update/delete data.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IAppData: IDataSource, IDataTarget
    {
        /// <summary>
        /// Create a new entity in the storage.
        /// </summary>
        /// <param name="contentTypeName">The type name</param>
        /// <param name="values">a dictionary of values to be stored</param>
        /// <param name="userName">the current user name - will be logged as the author</param>
        /// <param name="target">information if this new item is to be metadata for something</param>
        void Create(string contentTypeName, Dictionary<string, object> values, string userName = null, ITarget target = null);

        /// <summary>
        /// Create a bunch of new entities in one single call (much faster, because cache doesn't need to repopulate in the mean time).
        /// </summary>
        /// <param name="contentTypeName">The type name</param>
        /// <param name="multiValues">many dictionaries, each will become an own item when stored</param>
        /// <param name="userName">the current user name - will be logged as the author</param>
        /// <remarks>You can't create items which are metadata with this, for that, please use the Create-one overload</remarks>
        void Create(string contentTypeName, IEnumerable<Dictionary<string, object>> multiValues, string userName = null);

        /// <summary>
        /// Update an existing item.
        /// </summary>
        /// <param name="entityId">The item ID</param>
        /// <param name="values">a dictionary of values to be updated</param>
        /// <param name="userName">the current user name - will be logged as the author of the change</param>
        void Update(int entityId, Dictionary<string, object> values, string userName = null);

        /// <summary>
        /// Delete an existing item
        /// </summary>
        /// <param name="entityId">The item ID</param>
        /// <param name="userName">the current user name - will be logged as the author of the change</param>
        void Delete(int entityId, string userName = null);

        /// <summary>
        /// Metadata is an important feature of apps. <br/>
        /// So the App DataSource automatically provides direct access to the metadata system.
        /// This allows users of the App to query metadata directly through this object. 
        /// </summary>
        IMetadataSource Metadata { get; }
    }
}
