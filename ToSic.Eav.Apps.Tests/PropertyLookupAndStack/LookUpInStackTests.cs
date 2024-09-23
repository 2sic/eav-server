using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.LookUp;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;

[TestClass]
public class LookUpInStackTests: PropLookupStackBase
{
    [TestMethod]
    public void BasicDotNotation() => AreEqual(ChildJb1.Name, GetLookup().Get($"{FieldChildren}.{FieldName}"));

    [TestMethod]
    public void BasicColonNotation() => AreEqual(ChildJb1.Name, GetLookup().Get($"{FieldChildren}:{FieldName}"));

    private LookUpInStack GetLookup()
    {
        var lookup = new LookUpInStack("Settings", FirstJungleboy, PropReqSpecs.EmptyDimensions);
        return lookup;
    }
}