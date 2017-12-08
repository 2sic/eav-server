using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IEntity: IEntityLight, IPublish<IEntity>
    {
        object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false);

        string GetBestTitle(string[] dimensions);

        Dictionary<string, IAttribute> Attributes { get; }

        new IAttribute Title { get; }

        /// <summary>
        /// Gets an Attribute by its StaticName
        /// </summary>
        /// <param name="attributeName">StaticName of the Attribute</param>
        new IAttribute this[string attributeName] { get; }

        /// <summary>
        /// version of this entity in the repository
        /// </summary>
        int Version { get; }


        /// <summary>
        /// Get the metadata for this item
        /// </summary>
        /// <remarks>
        /// The metadata is either already prepared, from the same app, or from a remote app
        /// </remarks>
        IMetadataOfItem Metadata { get; }


        /// <summary>
        /// Dummy entry, to show better error message when it's being accessed because of breaking changes in EAV 4.5 / 2sxc 9.8, which 
        /// </summary>
        object Value { get; }
    }
}
