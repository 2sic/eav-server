﻿namespace ToSic.Eav.Apps.Internal.Ui;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public struct ContentTypeUiInfo
{
    public string Name;
    public string StaticName;
    public bool IsHidden;
    public IDictionary<string, object> Properties;
    public string Thumbnail;
    public bool IsDefault; // new, v13
}