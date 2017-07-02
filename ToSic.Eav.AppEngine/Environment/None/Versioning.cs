using System;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment.None
{
    public class Versioning : IEnvironmentVersioning
    {
        public bool Supported => false;

        public VersioningRequirements Requirements(int instanceId, string moreParamsIDontKnowYet) 
            => Enums.VersioningRequirements.DraftOptional;

        public void DoInsideVersioning(Action<bool> action)
        {
            var required = false;
            action.Invoke(required);
        }
    }
}
