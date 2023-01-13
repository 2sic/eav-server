using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data.PropertyLookup;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack
{
    [TestClass]
    public class PropLookupDicTest: PropertyLookupTestBase
    {
        private object LookupJungleboy(string fieldName) => GetResult(Jungleboy.Lookup, fieldName);
        private object LookupJohnDoe(string fieldName) => GetResult(JohnDoe.Lookup, fieldName);

        [TestMethod] public void JungleboyName() => AreEqual(Jungleboy.Name, LookupJungleboy(FieldName));
        [TestMethod] public void JungleboyNameCaseInsensitive() => AreEqual(Jungleboy.Name, LookupJungleboy(FieldName.ToUpper()));
        [TestMethod] public void JungleboyBirthday() => AreEqual(Jungleboy.Birthday, LookupJungleboy(FieldBirthday));
        [TestMethod] public void JungleboyDog() => IsNull(LookupJungleboy(FieldDog));
        [TestMethod] public void JungleboyUnknownProp() => IsNull(LookupJungleboy(UnusedField));

        [TestMethod] public void JungleboyChildren() => IsNotNull(LookupJungleboy(FieldChildren));
        [TestMethod] public void JungleboyChildrenIsListOfPropL() => IsInstanceOfType(LookupJungleboy(FieldChildren), typeof(IEnumerable<IPropertyLookup>));



        [TestMethod] public void JohnDoeName() => AreEqual(JohnDoe.Name, LookupJohnDoe(FieldName));
        [TestMethod] public void JohnDoeNameCaseInsensitive() => AreEqual(JohnDoe.Name, LookupJohnDoe(FieldName.ToUpper()));
        [TestMethod] public void JohnDoeBirthday() => AreEqual(JohnDoe.Birthday, LookupJohnDoe(FieldBirthday));
        [TestMethod] public void JohnDoeDog() => AreEqual(JohnDoe.Dog, LookupJohnDoe(FieldDog));
        [TestMethod] public void JohnDoeUnknownProp() => IsNull(LookupJohnDoe(UnusedField));

        [TestMethod] public void JohnDoeChildren() => IsNull(LookupJohnDoe(FieldChildren));

    }
}
