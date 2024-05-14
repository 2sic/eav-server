namespace ToSic.Eav.Security;

public static class GrantsToList
{
    public static List<Grants> AsSet(this Grants grant) => [grant];
}