using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.WebApi.Formats
{
    public class ContentTypeFieldInfo: IHasExternalI18n
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }
        public string InputType { get; set; }
        public string StaticName { get; set; }
        public bool IsTitle { get; set; }
        public int AttributeId { get; set; }
        public Dictionary<string, Dictionary<string, object>> Metadata { get; set; }
        public Dictionary<string, object> InputTypeConfig { get; set; }

        public string I18nKey { get; set; }
    }
}
