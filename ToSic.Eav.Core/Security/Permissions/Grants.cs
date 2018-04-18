namespace ToSic.Eav.Security.Permissions
{
    public enum Grants
    {
        Create = 'c',

        Read = 'r',

        Update = 'u',

        Delete = 'd',

        Develop = 'v',

        /// <summary>
        /// Full means everything EXCEPT for develop
        /// </summary>
        Full = 'f',

        Approve = 'a',

        Schema = '$',

        ReadDraft = 'ř',

        CreateDraft = 'č',

        UpdateDraft = 'ǔ',

        DeleteDraft = 'ď', // the "d" with caron looks a bit different
    }

}