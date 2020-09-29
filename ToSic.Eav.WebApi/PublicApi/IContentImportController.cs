using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IContentImportController
    {
        //ContentImportResultDto EvaluateContent(ContentImportArgsDto args);
        bool Import(EntityImportDto args);
        //ContentImportResultDto ImportContent(ContentImportArgsDto args);
    }
}