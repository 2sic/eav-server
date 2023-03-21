using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys
{
    // Note: ATM serves as Raw and as DTO, but should change soon.
    // once we adjust the front end to use the query
    public class AppStackDataRaw: RawEntityBase
    {
        public const string TypeName = "AppStack";
        public const string TitleFieldName = Data.Attributes.TitleNiceName;

        public static DataFactoryOptions Options = new DataFactoryOptions(typeName: TypeName, titleField: TitleFieldName);

        public AppStackDataRaw(PropertyDumpItem original)
        {
            Path = original.Path;
            Priority = original.SourcePriority;
            Source = original.SourceName;
            TotalResults = original.AllOptions?.GroupBy(i => i.SourceName)?.Count() ?? 0; // do not count "duplicate" by SourceName
            Type = original.Property.FieldType;
            Value = original.Property.Result;
        }

        public string Source { get; set; }
        
        public int Priority { get; set; }
        
        public string Path { get; set; }

        public object Value { get; set; }

        public string Type { get; set; }

        public int TotalResults { get; set; }

        public override Dictionary<string, object> Attributes(RawConvertOptions options)
        {
            var attributes = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { Data.Attributes.TitleNiceName, Path },
                { nameof(Source), Source },
                { nameof(Priority), Priority },
                { nameof(Type), Type },
                { nameof(TotalResults), TotalResults },
            };
            if (options.AddKey(nameof(Value)))
                attributes[nameof(Value)] = Value;
            return attributes;
        }
    }
}
