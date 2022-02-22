namespace ToSic.Eav.WebApi.Routing
{
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

    public class AppParts
    {
        public const string Auto = "auto";
        public const string Content = "content";
        public const string Data = "data"; // new, v13
        public const string Query = "query";
    }


    public class AppRoots
    {
        public const string AppAuto = Areas.App + "/" + AppParts.Auto;
        public const string AppNamed = Areas.App + "/" + TokensFramework.AppPath;
        public const string AppContentAuto = AppAuto + "/" + AppParts.Content;
        public const string AppContentNamed = AppNamed + "/" + AppParts.Content;
        public const string AppDataAuto = AppAuto + "/" + AppParts.Data; // new, v13
        public const string AppDataNamed = AppNamed + "/" + AppParts.Data; // new, v13
    }
}
