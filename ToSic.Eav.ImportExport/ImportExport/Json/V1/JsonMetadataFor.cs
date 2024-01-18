using ToSic.Lib.Documentation;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonMetadataFor
{
    /// <summary>
    /// The target type as a name - should be deprecated soon!
    /// </summary>
    public string Target;

    // #TargetTypeIdInsteadOfTarget
    /// <summary>
    /// The target type ID should replace the target soon, ATM they should co-exist
    /// </summary>
    public int TargetType;

    [JsonIgnore(Condition = WhenWritingNull)] public string String;
    [JsonIgnore(Condition = WhenWritingNull)] public Guid? Guid;
    [JsonIgnore(Condition = WhenWritingNull)] public int? Number;
        
    [PrivateApi("only used internally for now, name may change")]
    [JsonIgnore(Condition = WhenWritingNull)] public bool? Singleton;


    [PrivateApi("only used internally for now")]
    [JsonIgnore(Condition = WhenWritingNull)] public string Title;
}