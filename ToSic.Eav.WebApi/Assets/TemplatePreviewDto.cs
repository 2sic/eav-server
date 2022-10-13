using System.Text.Json.Serialization;

namespace ToSic.Eav.WebApi.Assets
{
    public class TemplatePreviewDto
    {
        public bool IsValid { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Error { get; set; }

        public string Preview { get; set; }
    }
}
