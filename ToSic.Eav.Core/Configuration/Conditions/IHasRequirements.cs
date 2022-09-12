using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public interface IHasRequirements
    {
        /// <summary>
        /// Optional requirements which are necessary for this feature to be used
        /// </summary>
        List<Condition> Requirements { get; }
    }
}
