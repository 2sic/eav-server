using System;
using ToSic.Eav.Data;
using ToSic.Eav.Data.ContentTypes.CodeAttributes;

namespace ToSic.Eav.Core.Tests.Data.ContentTypeFactoryTests;

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
internal class TestTypeWithSpecs: TestTypeWithSpecsEmpty
{
    [ContentTypeAttributeSpecs(Name = "NameMod", IsTitle = true)]
    public string Name { get; set; }

    [ContentTypeAttributeSpecs(Type = ValueTypes.Hyperlink)]
    public string Url { get; set; }

    public int Age { get; set; }

    public DateTime BirthDate { get; set; }

    internal const string IsAliveDescription = "This is to ensure the user is alive";
    [ContentTypeAttributeSpecs(Description = IsAliveDescription)]
    public bool IsAlive { get; set; }

}