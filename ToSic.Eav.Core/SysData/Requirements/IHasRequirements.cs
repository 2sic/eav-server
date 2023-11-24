using System.Collections.Generic;

namespace ToSic.Eav.SysData;

public interface IHasRequirements
{
    /// <summary>
    /// Optional requirements which are necessary for this feature to be used
    /// </summary>
    List<Requirement> Requirements { get; }
}