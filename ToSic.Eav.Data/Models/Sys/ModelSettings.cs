namespace ToSic.Eav.Models.Sys;

public record ModelSettings
{
    /// <summary>
    /// Instructs the created model to be strict about missing properties.
    /// Accessing the property at the entry level (top of the tree, not sub-properties) is required, the property must exist.
    /// </summary>
    public bool EntryPropIsRequired { get; init; } = true;

    /// <summary>
    /// This seems to instruct if child-objects should also be strict.
    /// </summary>
    public required bool ItemIsStrict { get; init; }

    /// <summary>
    /// Instructs the factory to create a mock object instead of a real one.
    /// </summary>
    public bool UseMock { get; init; } = false;

    /// <summary>
    /// Instructs the factory to filter out null items when converting a list.
    /// </summary>
    public bool DropNullItems { get; init; } = true;
}
