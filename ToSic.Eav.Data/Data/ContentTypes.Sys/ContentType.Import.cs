namespace ToSic.Eav.Data.ContentTypes.Sys;

partial record ContentType
{

    // special values just needed for import / save 
    // todo: try to place in a sub-object to un-clutter this ContentType object
    [PrivateApi]
    public required bool OnSaveSortAttributes { get; init; }

    [PrivateApi]
    public required string OnSaveUseParentStaticName { get; init; }

}