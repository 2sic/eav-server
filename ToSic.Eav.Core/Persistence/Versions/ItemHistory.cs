﻿using System;

namespace ToSic.Eav.Persistence.Versions;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ItemHistory
{
    public DateTime TimeStamp { get; set; }
    public string User { get; set; }
    public int ChangeSetId { get; set; }
    public int HistoryId { get; set; }
    public int VersionNumber { get; set; }
    public string Json { get; set; }
}