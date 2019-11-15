using System;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// Reference to target. Usually used on <see cref="IEntity"/> to define what thing it provides additional metadata for.
    /// Basically it contains all the references necessary to identify what it belongs to.
    /// </summary>
    [PublicApi]
	public interface ITarget
	{
        /// <summary>
        /// Determines if the current thing is used as Metadata.
        /// </summary>
        /// <returns>True if it's a metadata item, false if not. </returns>
        [Newtonsoft.Json.JsonIgnore]
        bool IsMetadata { get; }

        /// <summary>
        /// If this is metadata, then the target could be anything.
        /// This is an ID telling what kind of thing we're enhancing. 
        /// </summary>
        /// <returns>An ID from the system which registers all the types of things that can be described. See also <see cref="ITargetTypes"/>.</returns>
        int TargetType { get; }

        /// <summary>
        /// A string key identifying a target. 
        /// </summary>
        /// <returns>The string key of the target. Null if the identifier is not a string.</returns>
        string KeyString { get; }

        /// <summary>
        /// A number (int) key identifying a target. 
        /// </summary>
        /// <returns>The number key of the target. Null if the identifier is not a string.</returns>
        int? KeyNumber { get; }

        /// <summary>
        /// A GUID key identifying a target. 
        /// </summary>
        /// <returns>The GUID key of the target. Null if the identifier is not a string.</returns>
        Guid? KeyGuid { get; }

    }

}