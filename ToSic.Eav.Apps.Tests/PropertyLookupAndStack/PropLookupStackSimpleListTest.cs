using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack
{
    [TestClass]
    public class PropLookupStackSimpleListTest: PropLookupStackBase
    {
        #region Basic Values both with Find as well as with Path

        [TestMethod] public void JungleboyFirstName() => AreEqual(Jungleboy.Name, FindInJbJd(FieldName));
        [TestMethod] public void JungleboyFirstNamePath() => AreEqual(Jungleboy.Name, FindInJungleFirstPath(FieldName));
        [TestMethod] public void JungleboyFirstBirthday() => AreEqual(Jungleboy.Birthday, FindInJbJd(FieldBirthday));
        [TestMethod] public void JungleboyFirstBirthdayPath() => AreEqual(Jungleboy.Birthday, FindInJungleFirstPath(FieldBirthday));
        [TestMethod] public void JungleboyFirstDog() => AreEqual(JohnDoe.Dog, FindInJbJd(FieldDog));
        [TestMethod] public void JungleboyFirstDogPath() => AreEqual(JohnDoe.Dog, FindInJungleFirstPath(FieldDog));
        [TestMethod] public void JungleboyFirstUnusedField() => IsNull(FindInJbJd(UnusedField));
        [TestMethod] public void JungleboyFirstUnusedFieldPath() => IsNull(FindInJungleFirstPath(UnusedField));

        #endregion

        #region Prefix Tests

        // These tests will check if the prefix is ignored correctly
        // Reason is that if we have a settings stack, people could often use Settings.Get("Settings.GoogleApi") because they copy the key from certain sources

        [TestMethod] public void IgnorePrefixInPath_Basic() => AreEqual(Jungleboy.Name, FindInJungleFirstPath($"{StackName}.{FieldName}"));
        [TestMethod] public void IgnorePrefixInPath_ChildName() => AreEqual(ChildJb1.Name, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldName}"));
        [TestMethod] public void IgnorePrefixInPath_ChildDog() => AreEqual(ChildJb2.Dog, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldDog}"));
        [TestMethod] public void IgnorePrefixInPath_ChildCat() => AreEqual(ChildOfJaneDoe.Cat, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldCat}"));

        [TestMethod] public void IgnorePrefixInPath_GrandChildName() => AreEqual(GrandchildJb.Name, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldChildren}.{FieldName}"));


        #endregion

        [TestMethod] public void JungleboyChildrenIsListOfPropL() => IsInstanceOfType(FindInJbJd(FieldChildren), typeof(IEnumerable<IPropertyLookup>));
        [TestMethod] public void JungleboyChildrenPathIsListOfPropL() => IsInstanceOfType(FindInJungleFirstPath(FieldChildren), typeof(IEnumerable<IPropertyLookup>));
        [TestMethod] public void JungleboyChildrenIsStackNav() => IsInstanceOfType(FindInJbJd(FieldChildren), typeof(IEnumerable<PropertyLookupWithStackNavigation>));

        [TestMethod] public void JungleboyFirst_ChildName() => AreEqual(ChildJb1.Name, FindInJungleFirstPath($"{FieldChildren}.{FieldName}"));
        [TestMethod] public void JungleboyFirst_ChildDog() => AreEqual(ChildJb2.Dog, FindInJungleFirstPath($"{FieldChildren}.{FieldDog}"));
        [TestMethod] public void JungleboyFirst_ChildCat() => AreEqual(ChildOfJaneDoe.Cat, FindInJungleFirstPath($"{FieldChildren}.{FieldCat}"));

        [TestMethod] public void JungleboyFirst_GrandChildName() => AreEqual(GrandchildJb.Name, FindInJungleFirstPath($"{FieldChildren}.{FieldChildren}.{FieldName}"));


        [TestMethod]
        public void JungleboyChildName()
        {
            var result = GetJungleboyChildrenStack(FieldChildren);
            AreEqual(ChildJb1.Name, GetResult(result.Substack, FieldName, result.path));
        }

        [TestMethod]
        public void JungleboyChildDogOfSecondChild()
        {
            var result = GetJungleboyChildrenStack(FieldChildren);
            AreEqual(ChildJb2.Dog, GetResult(result.Substack, FieldDog, result.path));
        }

        [TestMethod]
        public void JungleboyChildCatOfJanesKids()
        {
            var initialResult = GetJungleboyChildrenStack(FieldChildren);
            var result = GetRequest(initialResult.Substack, FieldCat, initialResult.path);
            AreEqual(ChildOfJaneDoe.Cat, result.Result);
        }

        private (IPropertyLookup Substack, PropertyLookupPath path) GetJungleboyChildrenStack(string fieldName)
        {
            var path = new PropertyLookupPath();
            var requestResult = GetRequest(FirstJungleboy, fieldName, path);
            IsNotNull(requestResult);
            var result = requestResult.Result as IEnumerable<IPropertyLookup>;
            IsNotNull(result);

            IsTrue(result.Any());
            requestResult.Result = result.First();
            return (result.First(), requestResult.Path);
        }

        [TestMethod] public void TestJohnFirstName() => AreEqual(JohnDoe.Name, FindInJohnDoe(FieldName));
        [TestMethod] public void TestJohnFirstBirthday() => AreEqual(JohnDoe.Birthday, FindInJohnDoe(FieldBirthday));
        [TestMethod] public void TestJohnFirstDog() => AreEqual(JohnDoe.Dog, FindInJohnDoe(FieldDog));
        [TestMethod] public void TestJohnFirstUnusedField() => IsNull(FindInJohnDoe(UnusedField));
        [TestMethod] public void TestJohnChildrenOfJungleboy() => IsNotNull(FindInJohnDoe(FieldChildren));

    }
}
