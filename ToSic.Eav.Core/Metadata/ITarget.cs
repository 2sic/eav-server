using System;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Metadata;

/// <summary>
/// Reference to target. Usually used on <see cref="IEntity"/> to define what thing it provides additional metadata for.
/// Basically it contains all the references necessary to identify what it belongs to.
/// </summary>
[PrivateApi("was public till 16.09")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
    [JsonPropertyName("Target")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    int TargetType { get; }

    /// <summary>
    /// A string key identifying a target. 
    /// </summary>
    /// <returns>The string key of the target. Null if the identifier is not a string.</returns>
    [JsonPropertyName("String")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string KeyString { get; }

    /// <summary>
    /// A number (int) key identifying a target. 
    /// </summary>
    /// <returns>The number key of the target. Null if the identifier is not a string.</returns>
    [JsonPropertyName("Number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? KeyNumber { get; }

    /// <summary>
    /// A GUID key identifying a target. 
    /// </summary>
    /// <returns>The GUID key of the target. Null if the identifier is not a string.</returns>
    [JsonPropertyName("Guid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Guid? KeyGuid { get; }

    [JsonIgnore]
    [PrivateApi("WIP v13")] string Title { get; set; }

    [PrivateApi("WIP for v14")]
    string[] Recommendations { get; set; }
}