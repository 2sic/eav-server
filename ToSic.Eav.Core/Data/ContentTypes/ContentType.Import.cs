﻿namespace ToSic.Eav.Data;

partial class ContentType
{

    // special values just needed for import / save 
    // todo: try to place in a sub-object to un-clutter this ContentType object
    [PrivateApi]
    public bool OnSaveSortAttributes { get; }

    [PrivateApi]
    public string OnSaveUseParentStaticName { get; }

}