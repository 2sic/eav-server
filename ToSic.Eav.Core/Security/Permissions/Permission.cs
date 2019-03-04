namespace ToSic.Eav.Security.Permissions
{
    /// <summary>
    /// Helper object for constants related to the permissions-type
    /// </summary>
    public class Permission
    {
        public const string TypeName = "PermissionConfiguration";
        public const string FieldIdentity = "Identity";
        public const string FieldCondition = "Condition";
        public const string FieldGrant = "Grant";
        public const string FieldOwner = "Owner"; // todo: unclear - this probably should be a global field name
    }
}
