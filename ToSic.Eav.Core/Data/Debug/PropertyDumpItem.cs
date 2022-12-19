using System.Collections.Generic;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Debug
{
    [PrivateApi]
    public class PropertyDumpItem
    {
        public const string Separator = ".";

        public static bool ShouldStop(string path) => path?.Length > 200;

        public static PropertyDumpItem DummyErrorShouldStop(string path)
        {
            var errPath = path + Separator + "ErrorTooDeep";
            return new PropertyDumpItem
            {
                Path = errPath,
                Property = new PropReqResult("error", new PropertyLookupPath().Add(errPath))
                {
                    FieldType = "Todo",
                    Name = "error",
                    Result = "error"
                }
            };
        }

        /// <summary>
        /// The source of this item
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// The priority of this source - for proper sorting / priorities
        /// </summary>
        public int SourcePriority { get; set; }

        /// <summary>
        /// Path to this property
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Result data of the property
        /// </summary>
        public PropReqResult Property { get; set; }

        public List<PropertyDumpItem> AllOptions { get; set; }
    }
}
