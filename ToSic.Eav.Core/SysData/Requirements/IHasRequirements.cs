﻿using System.Collections.Generic;

namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IHasRequirements
{
    /// <summary>
    /// Optional requirements which are necessary for this feature to be used
    /// </summary>
    List<Requirement> Requirements { get; }
}