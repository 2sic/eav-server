namespace ToSic.Eav.Models;

[WorkInProgressApi("WIP v22")]
public record ToModelOptions
{
    // Next...

    /// <summary>
    /// allow conversion even if the Content-Type of the entity doesn't match the type specified in the target model type
    /// </summary>
    public ModelTypeCheck TypeNameCheck { get; init; } = ModelTypeCheck.Strict;
    
    
    // later...
    
    public ModelNullHandling DataNull { get; init; }
    
    public enum ModelTypeCheck
    {
        Skip,
        Strict,
    }
}
