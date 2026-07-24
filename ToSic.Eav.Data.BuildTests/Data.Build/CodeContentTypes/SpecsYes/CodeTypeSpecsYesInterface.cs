using ToSic.Eav.Data.Sys.ContentTypes;
using static ToSic.Eav.Data.Build.CodeContentTypes.SpecsYes.CodeTypeSpecsConstants;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ToSic.Eav.Data.Build.CodeContentTypes.SpecsYes;

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public interface ICodeTypeSpecsYesInterface
{
   
    [ContentTypeAttributeSpecs(Description = IdAndGuidDescription)]
    public int Id { get; set; }

    [ContentTypeAttributeSpecs(Description = IdAndGuidDescription)]
    public Guid Guid { get; set; }

    /// <summary>
    /// System property, but without additional specs
    /// </summary>
    public DateTime Created { get; set; }

    [ContentTypeAttributeSpecs(Name = NameAttrSpecsNameModified, IsTitle = true)]
    public string Name { get; set; }


    [ContentTypeAttributeSpecs(Type = ValueTypes.Hyperlink)]
    public string Url { get; set; }

    public int Age { get; set; }

    public DateTime BirthDate { get; set; }

    /// <summary>
    /// The description is usually not public, but public here since the tests is elsewhere
    /// </summary>
    [ContentTypeAttributeSpecs(Description = IsAliveDescription)]
    public bool IsAlive { get; set; }


    [ContentTypeAttributeIgnore]
    public string IgnoreThis { get; set; }

    //private string PrivateProperty { get; set; }

    internal string InternalProperty { get; set; }
}

