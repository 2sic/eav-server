using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using static ToSic.Eav.Data.Build.CodeContentTypes.CodeTypeWithSpecsEmpty;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// This is a record version of the <see cref="CodeTypeWithSpecsClass"/> class, which is used to define a content type with specific attributes and specifications.
/// </summary>
/// <param name="Id"></param>
/// <param name="Guid"></param>
/// <param name="Created">System property, but without additional specs</param>
/// <param name="IgnoreThis"></param>
[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public record CodeTypeWithSpecsRecord(

    [property: ContentTypeAttributeSpecs(Description = CodeTypeWithSpecsClass.IdAndGuidDescription)]
    int Id,

    [property: ContentTypeAttributeSpecs(Description = CodeTypeWithSpecsClass.IdAndGuidDescription)]
    Guid Guid,

    DateTime Created,

    [property: ContentTypeAttributeSpecs(Name = CodeTypeWithSpecsClass.NameSpecsName, IsTitle = true)]
    string Name,

    [property: ContentTypeAttributeSpecs(Type = ValueTypes.Hyperlink)]
    string Url,

    int Age,

    DateTime BirthDate,

    [property: ContentTypeAttributeSpecs(Description = CodeTypeWithSpecsClass.IsAliveDescription)]
    bool IsAlive,

    [property: ContentTypeAttributeIgnore] string IgnoreThis
)
{
    private string PrivateProperty { get; set; }

    internal string InternalProperty { get; set; }
}

public record CodeTypeWithSpecsRecordConvertible(int Id, Guid Guid, DateTime Created, string Name, string Url, int Age, DateTime BirthDate, bool IsAlive, string IgnoreThis)
    : CodeTypeWithSpecsRecord(Id, Guid, Created, Name, Url, Age, BirthDate, IsAlive, IgnoreThis),
        IConvertibleToRawEntity
{
    
}