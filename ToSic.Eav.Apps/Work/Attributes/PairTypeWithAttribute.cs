﻿using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PairTypeWithAttribute
{
    public IContentType Type { get; set; }
    public IContentTypeAttribute Attribute { get; set; }
}