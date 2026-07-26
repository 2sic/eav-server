using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Sys.ContentTypes;
using static ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes.CodeTypeSpecsConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes;

/// <summary>
/// This is a record version of the <see cref="CodeTypeSpecsYesClass"/> class, which is used to define a content type with specific attributes and specifications.
/// </summary>
/// <param name="Id"></param>
/// <param name="Guid"></param>
/// <param name="Created">System property, but without additional specs</param>
/// <param name="IgnoreThis"></param>
[ContentType(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public record CodeTypeSpecsYesRecord(

    [property: ContentTypeField(Description = IdAndGuidDescription)]
    int Id,

    [property: ContentTypeField(Description = IdAndGuidDescription)]
    Guid Guid,

    DateTime Created,

    [property: ContentTypeField(Name = NameAttrSpecsNameModified, IsTitle = true)]
    string Name,

    [property: ContentTypeField(Type = ValueTypes.Hyperlink)]
    string Url,

    int Age,

    DateTime BirthDate,

    [property: ContentTypeField(Description = IsAliveDescription)]
    bool IsAlive,

    [property: ContentTypeFieldIgnore] string IgnoreThis
)
{
    private string PrivateProperty { get; set; }

    internal string InternalProperty { get; set; }
}
