using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Run
{
    public interface IPage : IWebResource
    {
        /// <summary>
        /// These parameters can reconfigure what view is used or change
        /// </summary>
        [PrivateApi("wip")] List<KeyValuePair<string, string>> Parameters { get; set; }

    }
}
