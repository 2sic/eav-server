namespace ToSic.Eav.Models;

[WorkInProgressApi("WIP v22")]
public record ToModelOptions
{
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
    
    
    public DataNullHandling NullHandling { get; init; } = DataNullHandling.Undefined;
    
    public enum ModelTypeCheck
    {
        Skip,
        Strict,
    }

    
    public enum DataNullHandling
    {
        /// <summary>
        /// Represents an undefined state.
        /// Will be treated as Default, but can be used to detect if the caller explicitly set it or not.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// If original data is null, return null.
        /// This is the default behavior.
        /// </summary>
        AsNull = 1 << 0,

        /// <summary>
        /// If original data is null, throw an exception.
        /// </summary>
        Throw = 1 << 1,

        /// <summary>
        /// If original data is null, try to return a model, unless the model says otherwise.
        /// </summary>
        ConvertTry = 1 << 2,

        /// <summary>
        /// If original data is null, try to return a model, unless the model says otherwise - in which case throw.
        /// </summary>
        ConvertOrThrow = 1 << 3,

        /// <summary>
        /// If original data is null, force return a model, even if the model may not be able to handle it.
        /// This is a very aggressive option and should only be used if you are sure that the model can handle null sources, or if you want to force it to do so for testing purposes.
        /// </summary>
        ConvertForce = 1 << 4,
    }

    internal static DataNullHandling DataNullPreserveOrSet(ToModelOptions? options, DataNullHandling preferred)
        => options is null or { NullHandling: DataNullHandling.Undefined }
            ? preferred
            : options.NullHandling;
}
