using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// This interface provides a standard for accessing hidden metadata items
    /// We need it, because MetadataOf usually hides permissions in normal access
    /// but when importing data, the items should be accessed to store in the DB
    /// </summary>
    public interface IMetadataWithHiddenItems
    {
        /// <summary>
        /// The complete list of metadata items, incl. the hidden ones
        /// </summary>
        List<IEntity> AllWithHidden { get; }
    }
}
