namespace ToSic.Eav.WebApi.Sys.Routing;

/// <summary>
/// These are all the root paths used in the API
/// Basically anything after the system api-root, like /api/2sxc/* (in DNN) or /#/api/2sxc/* (in Oqtane)
/// </summary>
public class Areas
{
    public const string App = "app";
    public const string Sys = "sys";
    public const string Cms = "cms";
    public const string Admin = "admin";
}

public class AreaRoutes
{
    public const string App = $"{Areas.App}/[controller]/[action]";
    public const string Sys = $"{Areas.Sys}/[controller]/[action]";
    public const string Cms = $"{Areas.Cms}/[controller]/[action]"; // new, v13
    public const string Admin = $"{Areas.Admin}/[controller]/[action]";
}


public class AppParts
{
    public const string Auto = "auto";
    public const string Content = "content";
    public const string Data = "data"; // new, v13
    public const string Query = "query";
}
