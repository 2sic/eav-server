using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeWithSpecs: CodeTypeWithSpecsEmpty
{
    [ContentTypeAttributeSpecs(Description = "DO NOT USE. This is a temporary, random ID calculated at runtime and will return different values all the time.")]
    public int Id { get; set; }

    [ContentTypeAttributeSpecs(Description = "DO NOT USE. This is a temporary, random ID calculated at runtime and will return different values all the time.")]
    public Guid Guid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// System property, but without additional specs
    /// </summary>
    public DateTime Created { get; set; }

    [ContentTypeAttributeSpecs(Name = "NameMod", IsTitle = true)]
    public string Name { get; set; }

    [ContentTypeAttributeSpecs(Type = ValueTypes.Hyperlink)]
    public string Url { get; set; }

    public int Age { get; set; }

    public DateTime BirthDate { get; set; }

    /// <summary>
    /// The description is usually not public, but public here since the tests is elsewhere
    /// </summary>
    public const string IsAliveDescription = "This is to ensure the user is alive";
    [ContentTypeAttributeSpecs(Description = IsAliveDescription)]
    public bool IsAlive { get; set; }


    [ContentTypeAttributeIgnore]
    public string IgnoreThis { get; set; }

    private string PrivateProperty { get; set; }

    internal string InternalProperty { get; set; }
}