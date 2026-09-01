namespace ToSic.Sys.Security.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class GrantsToList
{
    public static List<Grants> AsSet(this Grants grant) => [grant];
}