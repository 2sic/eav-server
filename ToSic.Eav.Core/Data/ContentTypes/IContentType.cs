using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents a Content Type information (the schema) used for <see cref="IEntity"/> objects.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public interface IContentType: IAppIdentityLight, IHasMetadata
	{
        /// <summary>
        /// Gets the Display Name of the Content Type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Static name - can be a GUID or a system-term for special types
        /// </summary>
        /// <remarks>being deprecated in V13, to be replaced with NameId</remarks>
        [Obsolete("Deprecated in v13, please use NameId instead")]
        string StaticName { get; }

        /// <summary>
        /// A unique id/name of the content-type. Previously called StaticName.
        /// </summary>
        /// <remarks>New in v13</remarks>
        string NameId { get; }

        /// <summary>
        /// The content-type description
        /// </summary>
        [Obsolete("Obsolete in v12, used to contain the description, which is now in the metadata")]
        string Description { get; }

        /// <summary>
        /// Get the scope of the Content Type (like sections in a DB)
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Get the id of the Content Type - you usually don't need this!
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Old name for Id, please use Id instead
        /// </summary>
        /// <remarks>Deprecated in v13</remarks>
        [Obsolete("Deprecated in V13, please use Id instead.")]
        int ContentTypeId { get; }


        /// <summary>
        /// Dictionary with all Attribute Definitions
        /// </summary>
        IList<IContentTypeAttribute> Attributes { get; set; }

        /// <summary>
        /// A simple indexer to get an attribute
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>The <see cref="IContentTypeAttribute"/> of the field name</returns>
        IContentTypeAttribute this[string fieldName] { get; }

        /// <summary>
        /// Information where the Content-Type was stored (file system, DB, etc.)
        /// </summary>
        RepositoryTypes RepositoryType { get; }

        /// <summary>
        /// Information / ID / URL to this content-type where it was stored in the repository
        /// </summary>
        string RepositoryAddress { get; }

        /// <summary>
        /// Determines if the data for this type is dynamic (spontaneously created) or real an EAV (split into sql-tables) or json somewhere
        /// To detect if it's just a global json-type, find out what repository the type is from (RepositoryType)
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        /// Get the metadata for this content-type
        /// </summary>
        /// <remarks>
        /// The metadata is either already prepared, from the same app, or from a remote app
        /// </remarks>
        new ContentTypeMetadata Metadata { get; }

        /// <summary>
        /// Check if this type is the same as a name given.
        /// Will check both the name and the static name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
	    bool Is(string name);


        #region WIP 12.03 / 13.02

        /// <summary>
        /// This internal property says which of the content-type properties
        /// should be used for DynamicChildren, enabling Content.Lightbox.xyz instead of Content.Items.Lightbox.xyz
        /// </summary>
        [PrivateApi("WIP 12.03")] 
        string DynamicChildrenField { get; }

        #endregion

        #region WIP v13 Shared Content Types should be passed around in IContentType defs

        [PrivateApi("very internal functionality")]
        bool AlwaysShareConfiguration { get; }

        #endregion
    }
}
