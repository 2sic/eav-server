using System.Collections.Generic;
using static ToSic.Eav.Security.Permissions.Grants;

namespace ToSic.Eav.Security.Permissions
{
    public static class GrantSets
    {
        public static List<Grants> ReadAnything = new List<Grants>
        {
            Read,
            ReadDraft
        };


        public static List<Grants> ReadDrafts = new List<Grants>
        {
            Approve,
            DeleteDraft,
            ReadDraft,
            UpdateDraft, 
            Delete,
            Develop,
            Full,
            Update
        };

        public static List<Grants> WriteSomething = new List<Grants>
        {
            Delete,
            DeleteDraft,
            CreateDraft,
            Develop,
            Full,
            Approve,
            UpdateDraft,
            Update
        };
    }
}
