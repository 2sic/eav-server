using ToSic.Eav.Data.ContentTypes.Sys;

namespace ToSic.Eav.Data.Builder.ClassesWithTypeDecoration;

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
internal class TestTypeWithSpecs: TestTypeWithSpecsEmpty
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

    internal const string IsAliveDescription = "This is to ensure the user is alive";
    [ContentTypeAttributeSpecs(Description = IsAliveDescription)]
    public bool IsAlive { get; set; }


    [ContentTypeAttributeIgnore]
    public string IgnoreThis { get; set; }

    private string PrivateProperty { get; set; }

    internal string InternalProperty { get; set; }
}