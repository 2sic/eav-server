using ToSic.Eav.Data.Debug;

namespace ToSic.Eav.WebApi.Dto
{
    public class StackInfoDto
    {
        public StackInfoDto(PropertyDumpItem original)
        {
            Path = original.Path;
            Priority = original.SourcePriority;
            Source = original.SourceName;
            TotalResults = original.AllOptions?.Count ?? 0;
            Type = original.Property.FieldType;
            Value = original.Property.Result;
        }

        public string Source { get; set; }
        
        public int Priority { get; set; }
        
        public string Path { get; set; }

        public object Value { get; set; }

        public string Type { get; set; }

        public int TotalResults { get; set; }
    }
}
