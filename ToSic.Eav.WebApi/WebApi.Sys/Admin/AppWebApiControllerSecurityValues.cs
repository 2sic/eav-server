using System.Text.Json;
using ToSic.Eav.WebApi.Sys.ApiExplorer;

namespace ToSic.Eav.WebApi.Sys.Admin;

internal static class AppWebApiControllerSecurityValues
{
    internal static Dictionary<string, object?> ToDictionary(ApiSecurityDto security)
        => new()
        {
            [nameof(ApiSecurityDto.IgnoreSecurity)] = security.IgnoreSecurity,
            [nameof(ApiSecurityDto.AllowAnonymous)] = security.AllowAnonymous,
            [nameof(ApiSecurityDto.RequireVerificationToken)] = security.RequireVerificationToken,
            [nameof(ApiSecurityDto.RequireContext)] = security.RequireContext,
            [nameof(ApiSecurityDto.View)] = security.View,
            [nameof(ApiSecurityDto.Edit)] = security.Edit,
            [nameof(ApiSecurityDto.Admin)] = security.Admin,
            [nameof(ApiSecurityDto.SuperUser)] = security.SuperUser,
        };
}
