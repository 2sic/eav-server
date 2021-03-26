using System;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Shared properties of IDataSource and IDataTarget
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IDataPartShared
    {
        /// <summary>
        /// Internal ID usually from persisted configurations IF the configuration was build from an pre-stored query.
        /// </summary>
        /// <returns>The guid of this data source which identifies the configuration <see cref="IEntity"/> of the data source.</returns>
        Guid Guid { get; set; }

        /// <summary>
        /// Name of this DataSource - not usually relevant.
        /// </summary>
        /// <returns>Name of this source.</returns>
        string Name { get; }

        
        [PrivateApi("still experimental, trying to get more info into debugging")]
        string Label { get; set; }
    }
}
