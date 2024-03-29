﻿namespace ToSic.Eav.Apps.Internal.Ui;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public struct TemplateUiInfo
{
    public int TemplateId;
    public string Name;
    public string ContentTypeStaticName;
    public bool IsHidden;
    public string Thumbnail;
    public bool IsDefault; // new, v13
}