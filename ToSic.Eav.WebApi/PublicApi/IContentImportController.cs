using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IContentImportController
    {
        ContentImportResult EvaluateContent(ContentImportArgs args);
        bool Import(EntityImport args);
        ContentImportResult ImportContent(ContentImportArgs args);
    }
}