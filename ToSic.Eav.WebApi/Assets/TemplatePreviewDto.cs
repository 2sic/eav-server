using Newtonsoft.Json;

namespace ToSic.Eav.WebApi.Assets
{
    public class TemplatePreviewDto
    {
        public bool IsValid { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        public string Preview { get; set; }
    }
}
