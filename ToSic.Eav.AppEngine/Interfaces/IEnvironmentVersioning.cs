using System;
using ToSic.Eav.Apps.Enums;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironmentVersioning
    {
        //bool Supported { get; }

        VersioningRequirements Requirements(int instanceId, string moreParamsIDontKnowYet);


        /// <summary>
        /// Wraps an action and performs pre/post processing related to versioning of the environment
        /// </summary>
        /// <param name="action"></param>
        void DoInsideVersioning(Action<bool> action);



    }
}
