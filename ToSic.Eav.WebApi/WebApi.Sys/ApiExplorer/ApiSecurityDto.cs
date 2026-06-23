// ReSharper disable InconsistentNaming

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

public class ApiSecurityDto
{
    // TODO: @2rb - change call to use camelCase, and change this to normal PascalCase
    public bool ignoreSecurity { get; set; }
        
    public bool allowAnonymous { get; set; }

    public bool requireVerificationToken { get; set; }

    // TODO: @2rb - why underscore? find out if there is a reason, otherwise neutralize
    public bool _validateAntiForgeryToken { get; set; }
    public bool _autoValidateAntiforgeryToken { get; set; }
    public bool _ignoreAntiforgeryToken { get; set; }

    public bool view { get; set; }
    public bool edit { get; set; }
    public bool admin { get; set; }
    public bool superUser { get; set; }


    public bool requireContext { get; set; }
}