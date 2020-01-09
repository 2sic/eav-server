using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Foundation for interfaces which will enhance <see cref="EntityBasedType"/> which gets its data from an Entity. <br/>
    /// This is used for more type safety - so you base your interfaces - like IPerson on this,
    /// otherwise you're IPerson would be missing the Title, Id, Guid
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IEntityBasedType
    {
        /// <summary>
        /// The underlying entity. 
        /// </summary>
        /// <returns>The entity, or null if not provided</returns>
        IEntity Entity { get; }

        /// <summary>
        /// The title as string.
        /// </summary>
        /// <remarks>Can be overriden by other parts, if necessary.</remarks>
        /// <returns>The title, or an empty string if not available or not string-able</returns>
        string Title { get; }

        /// <summary>
        /// The entity id, as quick, nice accessor.
        /// </summary>
        /// <returns>The id, or 0 if no entity available</returns>
        int Id { get; }

        /// <summary>
        /// The entity guid, as quick, nice accessor. 
        /// </summary>
        /// <returns>The guid, or an empty-guid of no entity available</returns>
        Guid Guid { get; }
    }
}