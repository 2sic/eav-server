namespace ToSic.Eav.WebApi.Formats
{
    public class ContentImportResult
    {
        public bool Succeeded;

        public dynamic Detail;

        public ContentImportResult(bool succeeded, dynamic detail)
        {
            Succeeded = succeeded;
            Detail = detail;
        }
    }
}
