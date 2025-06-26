namespace ToSic.Sys.Security.Permissions;

public static class GrantsToList
{
    public static List<Grants> AsSet(this Grants grant) => [grant];
}