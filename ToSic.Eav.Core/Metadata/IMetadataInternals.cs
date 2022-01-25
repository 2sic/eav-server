using System.Collections.Generic;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// This interface provides a standard for accessing hidden metadata items
    /// We need it, because MetadataOf usually hides permissions in normal access
    /// but when importing data, the items should be accessed to store in the DB
    /// </summary>
    [PrivateApi("not sure yet how to publish this api, if at all")]
    public interface IMetadataInternals
    {
        /// <summary>
        /// Internal API to override metadata providing, for example when creating new entities before saving.
        /// </summary>
        /// <param name="items"></param>
        [PrivateApi]
        void Use(List<IEntity> items);

        /// <summary>
        /// The complete list of metadata items, incl. the hidden ones
        /// </summary>
        List<IEntity> AllWithHidden { get; }
    }
}
