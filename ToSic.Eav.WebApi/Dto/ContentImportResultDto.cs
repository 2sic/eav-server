// ReSharper disable MemberCanBePrivate.Global
namespace ToSic.Eav.WebApi.Dto;

public class ContentImportResultDto
{
    public bool Success;

    // Todo: should not be dynamic
    // but ATM it's either an error-array or an object containing stats
    public dynamic Detail;

    public ContentImportResultDto(bool success, dynamic detail)
    {
        Success = success;
        Detail = detail;
    }
}