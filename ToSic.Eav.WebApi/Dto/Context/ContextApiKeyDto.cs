using ToSic.Eav.Data;

namespace ToSic.Eav.WebApi.Dto
{
    /// <summary>
    /// API Keys to use in the UI - such as Google Maps, Google Translate etc.
    /// </summary>
    public class ContextApiKeyDto : IHasIdentityNameId
    {
        public string NameId { get; set; }

        public string ApiKey { get; set; }

        public bool IsDemo { get; set; }
    }
}
