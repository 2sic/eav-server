namespace ToSic.Eav.Models;

[WorkInProgressApi("WIP v22")]
public record ToModelOptions
{
    // Next...

    /// <summary>
    /// allow conversion even if the Content-Type of the entity doesn't match the type specified in the target model type
    /// </summary>
    public ModelTypeCheck TypeNameCheck { get; init; } = ModelTypeCheck.Strict;


    /// <summary>
    /// The name of the type to match.
    /// Or of the entity type to filter by.
    /// This value is used to select entities of a specific type.
    /// </summary>
    /// <remarks>
    /// Leave `null` for default to just use the type name specified by the model.
    /// </remarks>
    public string? TypeName { get; init; }
    
    
    
    public ModelResultHandling ResultHandling { get; init; } = ModelResultHandling.ModelNullAsNull;


    // later...

    public ModelNullHandling DataNull { get; init; }
    
    public enum ModelTypeCheck
    {
        Skip,
        Strict,
    }
    
    public enum ModelResultHandling
    {
        /// <summary>
        /// Return null if the model reports not being able to handle the data given to it.
        /// This is the default.
        /// </summary>
        ModelNullAsNull = 1 << 7,

        ModelNullSkip = 1 << 8,

        ModelNullThrows = 1 << 9,

        ModelNullAsModel = 1 << 10,
    }
}
