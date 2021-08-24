using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.Debug
{
    [PrivateApi]
    public class PropertyDumpItem
    {
        public const string Separator = ".";

        public static bool ShouldStop(string path) => path?.Length > 200;

        public static PropertyDumpItem DummyErrorShouldStop(string path) => new PropertyDumpItem
        {
            Path = path + Separator + "ErrorTooDeep",
            Property = new PropertyRequest()
            {
                FieldType = "Todo",
                Name = "error",
                Result = "error"
            }
        };

        public string Source { get; set; }
        public string Path { get; set; }
        public PropertyRequest Property { get; set; }
    }
}
