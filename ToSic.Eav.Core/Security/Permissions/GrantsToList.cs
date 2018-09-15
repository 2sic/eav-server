using System.Collections.Generic;

namespace ToSic.Eav.Security.Permissions
{
    public static class GrantsToList
    {
        public static List<Grants> AsSet(this Grants grant) => new List<Grants> {grant};
    }
}
