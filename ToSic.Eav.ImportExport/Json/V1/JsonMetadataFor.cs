using System;
using System.Text.Json.Serialization;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Json.V1
{
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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string String;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public Guid? Guid;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? Number;
        
        [PrivateApi("only used internally for now, name may change")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? Singleton;


        [PrivateApi("only used internally for now")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string Title;
    }
}
