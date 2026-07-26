using ToSic.Eav.Data.ContentTypes;
using static ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes.CodeTypeSpecsConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes;

[ContentType(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public interface ICodeTypeSpecsYesInterface
{
   
    [ContentTypeField(Description = IdAndGuidDescription)]
    public int Id { get; set; }

    [ContentTypeField(Description = IdAndGuidDescription)]
    public Guid Guid { get; set; }

    /// <summary>
    /// System property, but without additional specs
    /// </summary>
    public DateTime Created { get; set; }

    [ContentTypeField(Name = NameAttrSpecsNameModified, IsTitle = true)]
    public string Name { get; set; }


    [ContentTypeField(Type = ValueTypes.Hyperlink)]
    public string Url { get; set; }

    public int Age { get; set; }

    public DateTime BirthDate { get; set; }

    /// <summary>
    /// The description is usually not public, but public here since the tests is elsewhere
    /// </summary>
    [ContentTypeField(Description = IsAliveDescription)]
    public bool IsAlive { get; set; }


    [ContentTypeFieldIgnore]
    public string IgnoreThis { get; set; }

    //private string PrivateProperty { get; set; }

    internal string InternalProperty { get; set; }
}

