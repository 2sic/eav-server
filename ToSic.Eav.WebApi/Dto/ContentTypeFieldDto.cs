using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContentTypeFieldDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }
        public string InputType { get; set; }
        public string StaticName { get; set; }
        public bool IsTitle { get; set; }
        public int AttributeId { get; set; }
        public Dictionary<string, Dictionary<string, object>> Metadata { get; set; }
        public InputTypeInfo InputTypeConfig { get; set; }
    }
}
