using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Constants for data types
    /// </summary>
    [PrivateApi]
    public class DataTypes
    {
        public const string Boolean = "Boolean";
        public const string Number = "Number";
        public const string DateTime = "DateTime";
        public const string Entity = "Entity"; // todo: update all references with this as a constant
        public const string Hyperlink = "Hyperlink";
        public const string String = "String";
        
        // Don't call this "Empty" because it's too similar to "Entity" and could be overlooked when coding
        public static string VoidEmpty = "Empty";
    }
}
