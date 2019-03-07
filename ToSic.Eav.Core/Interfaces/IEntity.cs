using System.Collections.Generic;
using ToSic.Eav.Security.Permissions;

namespace ToSic.Eav.Interfaces
{
    public interface IEntity: IEntityLight, IPublish<IEntity>, IHasPermissions
    {
        object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false);
        T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks = false);

        object PrimaryValue(string attributeName, bool resolveHyperlinks = false);
        T PrimaryValue<T>(string attributeName, bool resolveHyperlinks = false);

        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <param name="dimensions">Array of dimensions to use in the lookup</param>
        /// <returns>The entity title as a string</returns>
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

        #region experimental IEntity Queryable / Quick
        List<IEntity> Children(string field = null, string type = null);
        List<IEntity> Parents(string type = null, string field = null);

        #endregion experimental
    }
}
