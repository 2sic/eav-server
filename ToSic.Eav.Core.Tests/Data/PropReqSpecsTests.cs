using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Core.Tests.Data;

[TestClass]
public class PropReqSpecsTests
{
    [TestMethod]
    public void DimensionsBlankAutoExpand()
    {
        var specs = new PropReqSpecs("TestField");
        CollectionAssert.AreEqual(new string[] { null }, specs.Dimensions);
    }

    [DataRow(new[] { (string)null }, null, DisplayName = "null, Auto Expand")]
    [DataRow(new[] { (string)null }, DisplayName = "null, Auto Expand")]
    [DataRow(new[] { (string)null }, [], DisplayName = "empty, Auto Expand")]
    [DataRow(new[] { "a", null }, "a", DisplayName = "Auto Expand")]
    [DataRow(new[] { "a", "b", null }, "a", "b", DisplayName = "Auto Expand")]
    [DataRow(new[] { "a", "b", null }, "a", "b", null, DisplayName = "Keep, don't add second null")]
    [DataRow(new[] { "a", null, "b", null }, "a", null, "b", DisplayName = "weird, shouldn't happen, but ...")]
    [TestMethod]
    public void Dimensions1AutoExpand(string[] expected, params string[] input)
    {
        var specs = new PropReqSpecs("TestField", input, false);
        CollectionAssert.AreEqual(expected, specs.Dimensions);
    }

    [DataRow(new string[] { },  DisplayName = "null, Auto Expand")]
    [DataRow(new[] { "a" }, "a", DisplayName = "a, no Auto Expand")]
    [TestMethod]
    public void DimensionsNoModUntouched(string[] expected, params string[] input)
    {
        var specs = new PropReqSpecs("TestField", input, DimsAreFinal: true);
        CollectionAssert.AreEqual(expected, specs.Dimensions);
    }


    [TestMethod]
    public void DimensionsWithNullUntouched()
    {
        var specs = new PropReqSpecs("TestField", ["a", null], true);
        CollectionAssert.AreEqual(new[] { "a", null }, specs.Dimensions);
    }
}