// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public class ApiSecurityDto
{
    public bool IgnoreSecurity { get; set; }
        
    public bool AllowAnonymous { get; set; }

    public bool RequireVerificationToken { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public bool ValidateAntiForgeryToken { get; set; }
    public bool AutoValidateAntiforgeryToken { get; set; }
    public bool IgnoreAntiforgeryToken { get; set; }

    public bool View { get; set; }
    public bool Edit { get; set; }
    public bool Admin { get; set; }
    public bool SuperUser { get; set; }


    public bool RequireContext { get; set; }

    /// <summary>
    /// Helper to merge two security DTOs
    /// </summary>
    /// <param name="controllerSecurity"></param>
    /// <param name="methodSecurity"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static ApiSecurityDto MergeSecurity(ApiSecurityDto controllerSecurity, ApiSecurityDto methodSecurity, ILog? log = default)
    {
        var l = log.Fn<ApiSecurityDto>();

        var ignoreSecurity = controllerSecurity.IgnoreSecurity || methodSecurity.IgnoreSecurity;
        var allowAnonymous = controllerSecurity.AllowAnonymous || methodSecurity.AllowAnonymous;
        var view = controllerSecurity.View || methodSecurity.View;
        var edit = controllerSecurity.Edit || methodSecurity.Edit;
        var admin = controllerSecurity.Admin || methodSecurity.Admin;
        var superUser = controllerSecurity.SuperUser || methodSecurity.SuperUser;
        var requireContext = controllerSecurity.RequireContext || methodSecurity.RequireContext;

        var requireVerificationToken =
            methodSecurity.ValidateAntiForgeryToken ||
            methodSecurity.AutoValidateAntiforgeryToken ||
            methodSecurity.IgnoreAntiforgeryToken
                ? methodSecurity.RequireVerificationToken
                : controllerSecurity.RequireVerificationToken;

        var result = new ApiSecurityDto
        {
            IgnoreSecurity = ignoreSecurity,
            AllowAnonymous = ignoreSecurity || (allowAnonymous && !view && !edit && !admin && !superUser),
            View = ignoreSecurity || ((allowAnonymous || view) && !edit && !admin && !superUser),
            Edit = ignoreSecurity || ((allowAnonymous || view || edit) && !admin && !superUser),
            Admin = ignoreSecurity || ((allowAnonymous || view || edit || admin) && !superUser),
            SuperUser = ignoreSecurity || allowAnonymous || view || edit || admin || superUser,
            RequireContext = !ignoreSecurity && requireContext,
            RequireVerificationToken = !ignoreSecurity && requireVerificationToken,
        };

        return l.ReturnAsOk(result);
    }
}