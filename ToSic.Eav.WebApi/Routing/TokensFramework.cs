namespace ToSic.Eav.WebApi.Routing
{
    /// <summary>
    /// These are tokens in the format {xyz} used in routes to identify a part
    /// </summary>
    public class TokensFramework
    {
        // Single Placeholders
        public const string Name = "{" + VarNames.Name + "}";
        public const string Stream = "{" + VarNames.Stream + "}";
        public const string Edition = "{" + VarNames.Edition + "}";
        public const string AppPath = "{" + VarNames.AppPath + "}";
        public const string Controller = "{" + VarNames.Controller + "}";
        public const string Action = "{" + VarNames.Action + "}";

        public const string ContentType = "{" + VarNames.ContentType + "}";
        public const string Id = "{" + VarNames.Id + "}";
        public const string Guid = "{" + VarNames.Guid + "}";
        public const string Field = "{" + VarNames.Field + "}";

        // Sets / Pairs of placeholders
        public const string SetControllerAction = Controller + "/" + Action;
        public const string SetTypeAndId = ContentType + "/" + Id;
        public const string SetTypeAndGuid = ContentType + "/" + Guid;
        public const string SetTypeGuidField = SetTypeAndGuid + "/" + Field;
        public const string SetTypeGuidFieldAction = SetTypeGuidField + "/" + Action;
    }
}
