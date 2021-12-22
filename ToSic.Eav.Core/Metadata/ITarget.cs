using System;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// Reference to target. Usually used on <see cref="IEntity"/> to define what thing it provides additional metadata for.
    /// Basically it contains all the references necessary to identify what it belongs to.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	public interface ITarget
	{
        /// <summary>
        /// Determines if the current thing is used as Metadata.
        /// </summary>
        /// <returns>True if it's a metadata item, false if not. </returns>
        [JsonIgnore]
        bool IsMetadata { get; }

        /// <summary>
        /// If this is metadata, then the target could be anything.
        /// This is an ID telling what kind of thing we're enhancing. 
        /// </summary>
        /// <remarks>
        /// - In 2sxc 8 - 12 this is called AssignmentObjectTypeId in the DB, but will change some day.
        /// - It must be an int, not a <see cref="TargetTypes"/> enum, because the DB could hold values which are not in the enum
        /// </remarks>
        /// <returns>An ID from the system which registers all the types of things that can be described. See also <see cref="ITargetTypes"/>.</returns>
        [JsonProperty("Target", NullValueHandling = NullValueHandling.Ignore)]
        int TargetType { get; }

        /// <summary>
        /// A string key identifying a target. 
        /// </summary>
        /// <returns>The string key of the target. Null if the identifier is not a string.</returns>
        [JsonProperty("String", NullValueHandling = NullValueHandling.Ignore)]
        string KeyString { get; }

        /// <summary>
        /// A number (int) key identifying a target. 
        /// </summary>
        /// <returns>The number key of the target. Null if the identifier is not a string.</returns>
        [JsonProperty("Number", NullValueHandling = NullValueHandling.Ignore)]
        int? KeyNumber { get; }

        /// <summary>
        /// A GUID key identifying a target. 
        /// </summary>
        /// <returns>The GUID key of the target. Null if the identifier is not a string.</returns>
        [JsonProperty("Guid", NullValueHandling = NullValueHandling.Ignore)]
        Guid? KeyGuid { get; }

        [JsonIgnore]
        [PrivateApi("WIP v13")] string Title { get; set; }
    }

}