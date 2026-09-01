using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Name = "Class",
    Guid = "6eb62f0c-e6b9-414d-999f-24fc972bdf9c"
)]
internal record ClassInfoRaw(
    int Id,
    string Name,
    [property: ContentTypeTitle] string FullName,
    string Assembly,
    string AssemblyQualifiedName,
    string Namespace
) : IRawEntityAutoConvert
{
    internal ClassInfoRaw(Type type, int index) : this(index, type.Name, type.FullName ?? "", type.Assembly.FullName, type.AssemblyQualifiedName ?? "", type.Namespace ?? "") { }
}
