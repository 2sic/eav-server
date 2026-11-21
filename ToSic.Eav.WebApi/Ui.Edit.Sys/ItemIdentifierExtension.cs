namespace ToSic.Eav.Ui.Edit.Sys;

public static class ItemIdentifierExtension
{
    /// <summary>
    /// Access the Parent GUID in scenarios where it must exist, or throw error
    /// </summary>
    /// <remarks>
    /// ParentOrError property was converted to extension method to avoid exceptions on STJ json deserialization
    /// </remarks>
    public static Guid GetParentEntityOrError(this ItemIdentifier itemIdentifier) =>
        itemIdentifier.Parent
        ?? throw new ArgumentNullException(nameof(itemIdentifier.Parent),
            @"Trying to access 'Parent' to save in list, but it's null");
}