using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi;

public interface IContentImportController
{
    bool Import(EntityImportDto args);
}