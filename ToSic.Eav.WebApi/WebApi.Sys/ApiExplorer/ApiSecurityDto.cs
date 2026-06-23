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
}