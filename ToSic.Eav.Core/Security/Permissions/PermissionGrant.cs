namespace ToSic.Eav.Security.Permissions
{
    public enum PermissionGrant
    {
        Create = 'c',
        Read = 'r',
        Update = 'u',
        Delete = 'd',
        Develop = 'v',
        Full = 'f',
        Approve = 'a',
        Schema = '$',
        ReadDraft = 'ř',
        CreateDraft = 'č',
        UpdateDraft = 'ǔ',
        DeleteDraft = 'ď' // the "d" with caron looks a bit different
    }

}