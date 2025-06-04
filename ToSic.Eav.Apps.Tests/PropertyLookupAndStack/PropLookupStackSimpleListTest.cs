using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;


public class PropLookupStackSimpleListTest: PropLookupStackBase
{
    #region Basic Values both with Find as well as with Path

    [Fact] public void JungleboyFirstName() => Equal(Jungleboy.Name, FindInJbJd(FieldName));
    [Fact] public void JungleboyFirstNamePath() => Equal(Jungleboy.Name, FindInJungleFirstPath(FieldName));
    [Fact] public void JungleboyFirstBirthday() => Equal(Jungleboy.Birthday, FindInJbJd(FieldBirthday));
    [Fact] public void JungleboyFirstBirthdayPath() => Equal(Jungleboy.Birthday, FindInJungleFirstPath(FieldBirthday));
    [Fact] public void JungleboyFirstDog() => Equal(JohnDoe.Dog, FindInJbJd(FieldDog));
    [Fact] public void JungleboyFirstDogPath() => Equal(JohnDoe.Dog, FindInJungleFirstPath(FieldDog));
    [Fact] public void JungleboyFirstUnusedField() => Null(FindInJbJd(UnusedField));
    [Fact] public void JungleboyFirstUnusedFieldPath() => Null(FindInJungleFirstPath(UnusedField));

    #endregion

    #region Prefix Tests

    // These tests will check if the prefix is ignored correctly
    // Reason is that if we have a settings stack, people could often use Settings.Get("Settings.GoogleApi") because they copy the key from certain sources

    [Fact] public void IgnorePrefixInPath_Basic() => Equal(Jungleboy.Name, FindInJungleFirstPath($"{StackName}.{FieldName}"));
    [Fact] public void IgnorePrefixInPath_ChildName() => Equal(ChildJb1.Name, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldName}"));
    [Fact] public void IgnorePrefixInPath_ChildDog() => Equal(ChildJb2.Dog, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldDog}"));
    [Fact] public void IgnorePrefixInPath_ChildCat() => Equal(ChildOfJaneDoe.Cat, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldCat}"));

    [Fact] public void IgnorePrefixInPath_GrandChildName() => Equal(GrandchildJb.Name, FindInJungleFirstPath($"{StackName}.{FieldChildren}.{FieldChildren}.{FieldName}"));


    #endregion

    [Fact]
    public void JungleboyChildrenIsListOfPropL() =>
        IsAssignableFrom<IEnumerable<IPropertyLookup>>(FindInJbJd(FieldChildren));

    [Fact]
    public void JungleboyChildrenPathIsListOfPropL() =>
        IsAssignableFrom<IEnumerable<IPropertyLookup>>(FindInJungleFirstPath(FieldChildren));

    [Fact] public void JungleboyChildrenIsStackNav() =>
        IsAssignableFrom<IEnumerable<PropertyLookupWithStackNavigation>>(FindInJbJd(FieldChildren));

    [Fact] public void JungleboyFirst_ChildName() => 
        Equal(ChildJb1.Name, FindInJungleFirstPath($"{FieldChildren}.{FieldName}"));
    [Fact] public void JungleboyFirst_ChildDog() =>
        Equal(ChildJb2.Dog, FindInJungleFirstPath($"{FieldChildren}.{FieldDog}"));
    [Fact] public void JungleboyFirst_ChildCat() =>
        Equal(ChildOfJaneDoe.Cat, FindInJungleFirstPath($"{FieldChildren}.{FieldCat}"));

    [Fact] public void JungleboyFirst_GrandChildName() =>
        Equal(GrandchildJb.Name, FindInJungleFirstPath($"{FieldChildren}.{FieldChildren}.{FieldName}"));


    [Fact]
    public void JungleboyChildName()
    {
        var result = GetJungleboyChildrenStack(FieldChildren);
        Equal(ChildJb1.Name, GetResult(result.Substack, FieldName, result.path));
    }

    [Fact]
    public void JungleboyChildDogOfSecondChild()
    {
        var result = GetJungleboyChildrenStack(FieldChildren);
        Equal(ChildJb2.Dog, GetResult(result.Substack, FieldDog, result.path));
    }

    [Fact]
    public void JungleboyChildCatOfJanesKids()
    {
        var initialResult = GetJungleboyChildrenStack(FieldChildren);
        var result = GetRequest(initialResult.Substack, FieldCat, initialResult.path);
        Equal(ChildOfJaneDoe.Cat, result.Result);
    }

    private (IPropertyLookup Substack, PropertyLookupPath path) GetJungleboyChildrenStack(string fieldName)
    {
        var path = new PropertyLookupPath();
        var requestResult = GetRequest(FirstJungleboy, fieldName, path);
        NotNull(requestResult);
        var result = requestResult.Result as IEnumerable<IPropertyLookup>;
        NotNull(result);

        True(result.Any());
        requestResult.Result = result.First();
        return (result.First(), requestResult.Path);
    }

    [Fact] public void TestJohnFirstName() => Equal(JohnDoe.Name, FindInJohnDoe(FieldName));
    [Fact] public void TestJohnFirstBirthday() => Equal(JohnDoe.Birthday, FindInJohnDoe(FieldBirthday));
    [Fact] public void TestJohnFirstDog() => Equal(JohnDoe.Dog, FindInJohnDoe(FieldDog));
    [Fact] public void TestJohnFirstUnusedField() => Null(FindInJohnDoe(UnusedField));
    [Fact] public void TestJohnChildrenOfJungleboy() => NotNull(FindInJohnDoe(FieldChildren));

}