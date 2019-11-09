using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Foundation for interfaces which will enhance <see cref="EntityBasedType"/> which gets its data from an Entity. <br/>
    /// This is used for more type safety - so you base your interfaces - like IPerson on this,
    /// otherwise you're IPerson would be missing the Title, Id, Guid
    /// </summary>
    [PublicApi]
    public interface IEntityBasedType
    {
        /// <summary>
        /// The underlying entity. 
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// The title as string.
        /// </summary>
        /// <remarks>Can be overriden by other parts, if necessary.</remarks>
        string Title { get; }

        /// <summary>
        /// The entity id, as quick, nice accessor.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The entity guid, as quick, nice accessor. 
        /// </summary>
        Guid Guid { get; }
    }
}