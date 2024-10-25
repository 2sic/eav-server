using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;

namespace ToSic.Eav.Core.Tests.Data.ContentTypeFactoryTests;

[TestClass]
public class ContentTypeFactoryAttributesTests : ContentTypeFactoryTestsBase
{
    private void AssertAttribute(IContentType ct, string name, ValueTypes type, bool isTitle = false, string description = default)
    {
        var attribute = ct.Attributes.FirstOrDefault(a => a.Name == name);
        Assert.IsNotNull(attribute, $"{name} null check");
        Assert.AreEqual(name, attribute.Name, $"{name} Name check");
        Assert.AreEqual(type, attribute.Type, $"{name} type check");
        Assert.AreEqual(isTitle, attribute.IsTitle, $"{name} IsTitle check");
        if (description != default)
            Assert.AreEqual(description, attribute.Metadata.GetBestValue<string>(AttributeMetadata.DescriptionField), $"{name} Description check");
    }

    [TestMethod]
    public void Attributes_NoSpec_Count() => Assert.AreEqual(4, Factory().Create(typeof(TestTypeNoSpecs)).Attributes.Count());

    [TestMethod]
    [DataRow(nameof(TestTypeNoSpecs.Name), ValueTypes.String)]
    [DataRow(nameof(TestTypeNoSpecs.Age), ValueTypes.Number)]
    [DataRow(nameof(TestTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [DataRow(nameof(TestTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => AssertAttribute(Factory().Create(typeof(TestTypeNoSpecs)), name, type);

    [TestMethod]
    public void Attributes_WithSpec_Count() => Assert.AreEqual(5, Factory().Create(typeof(TestTypeWithSpecs)).Attributes.Count());

    [TestMethod]
    [DataRow(nameof(TestTypeWithSpecs.Name) + "Mod", ValueTypes.String, true)]
    [DataRow(nameof(TestTypeWithSpecs.Url), ValueTypes.Hyperlink)]
    [DataRow(nameof(TestTypeWithSpecs.Age), ValueTypes.Number)]
    [DataRow(nameof(TestTypeWithSpecs.BirthDate), ValueTypes.DateTime)]
    [DataRow(nameof(TestTypeWithSpecs.IsAlive), ValueTypes.Boolean, false, TestTypeWithSpecs.IsAliveDescription)]
    public void AssertAttributeWithSpec(string name, ValueTypes type, bool isTitle = false, string description = default)
        => AssertAttribute(Factory().Create(typeof(TestTypeWithSpecs)), name, type, isTitle, description);

    /// <summary>
    /// Don't use properties which are private, internal or have the Ignore attribute
    /// </summary>
    /// <param name="name"></param>
    [TestMethod]
    [DataRow(nameof(TestTypeWithSpecs.IgnoreThis))]
    [DataRow(nameof(TestTypeWithSpecs.InternalProperty))]
    [DataRow("PrivateProperty")]
    public void Attributes_WithSpec_SkipIgnores(string name)
        => Assert.IsFalse(Factory().Create(typeof(TestTypeWithSpecs)).Attributes.Any(a => a.Name == name));

}
