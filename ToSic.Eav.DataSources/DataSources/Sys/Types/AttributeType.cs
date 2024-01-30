namespace ToSic.Eav.DataSources.Sys.Types;

internal interface IAttributeType
{
    string Type { get; }
    string Name { get; }
    string Title { get; }
    bool IsTitle { get; }
    bool IsBuiltIn { get; }
    int SortOrder { get; }
    IContentType ContentType { get; }
}