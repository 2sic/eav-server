// ReSharper disable MemberCanBePrivate.Global
namespace ToSic.Eav.WebApi.Sys.Dto;

public class ContentImportResultDto(bool success, dynamic detail)
{
    public bool Success = success;

    // Todo: should not be dynamic
    // but ATM it's either an error-array or an object containing stats
    public dynamic Detail = detail;
}