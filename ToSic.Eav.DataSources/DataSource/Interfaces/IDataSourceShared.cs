using System;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    /// <summary>
    /// Shared properties of <see cref="IDataSourceSource"/> and <see cref="IDataSourceTarget"/>
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IDataSourceShared
    {
        /// <summary>
        /// Internal ID usually from persisted configurations IF the configuration was build from an pre-stored query.
        /// </summary>
        /// <returns>The guid of this data source which identifies the configuration <see cref="IEntity"/> of the data source.</returns>
        [PrivateApi]
        Guid Guid { get; }

        /// <summary>
        /// Name of this DataSource - not usually relevant.
        /// </summary>
        /// <returns>Name of this source.</returns>
        [PrivateApi]
        string Name { get; }

        [PrivateApi("internal use only - for labeling data sources in queries to show in debugging")]
        string Label { get; }

        [PrivateApi]
        void AddDebugInfo(Guid? guid, string label);
    }
}
