namespace ToSic.Eav.WebApi.Formats
{
    public class QueryImport
    {
        public int AppId;

        public string ContentBase64;

        public string DebugInfo => $"app:{AppId} + base:{ContentBase64}";
    }
}
