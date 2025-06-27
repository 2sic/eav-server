namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonMetadataFor
{
    /// <summary>
    /// The target type as a name - should be deprecated soon!
    /// </summary>
    public string? Target { get; init; }

    // #TargetTypeIdInsteadOfTarget
    /// <summary>
    /// The target type ID should replace the target soon, ATM they should co-exist
    /// </summary>
    public int TargetType { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)] public string? String { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)] public Guid? Guid { get; init; }
    [JsonIgnore(Condition = WhenWritingNull)] public int? Number { get; init; }
        
    [PrivateApi("only used internally for now, name may change")]
    [JsonIgnore(Condition = WhenWritingNull)] public bool? Singleton { get; init; }


    [PrivateApi("only used internally for now")]
    [JsonIgnore(Condition = WhenWritingNull)] public string? Title { get; init; }
}