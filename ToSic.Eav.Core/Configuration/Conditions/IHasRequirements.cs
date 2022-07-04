using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public interface IHasRequirements
    {
        List<Condition> Requirements { get; }
    }
}
