namespace ToSic.Eav.Data.Sys.ContentTypes;

/// <summary>
/// todo
/// #SharedFieldDefinition
///
/// ATM just an empty dummy class
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record ContentTypeSysSettings
{
    /// <summary>
    /// WIP - would disable saving of entities in the UI
    /// </summary>
    /// <remarks>
    /// This is meant for content types which simply display information or do something in JS,
    /// but are never expected to be persisted.
    /// </remarks>
    private bool EntitySave { get; init; } = true;
}