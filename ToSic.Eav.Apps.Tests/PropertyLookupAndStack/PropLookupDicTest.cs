using ToSic.Eav.Data.Sys;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;


public class PropLookupDicTest: PropertyLookupTestBase
{
    private object LookupJungleboy(string fieldName) =>
        GetResult(Jungleboy.Lookup, fieldName);

    private object LookupJohnDoe(string fieldName) =>
        GetResult(JohnDoe.Lookup, fieldName);

    [Fact] public void JungleboyName() =>
        Equal(Jungleboy.Name, LookupJungleboy(FieldName));

    [Fact] public void JungleboyNameCaseInsensitive() =>
        Equal(Jungleboy.Name, LookupJungleboy(FieldName.ToUpper()));

    [Fact] public void JungleboyBirthday() =>
        Equal(Jungleboy.Birthday, LookupJungleboy(FieldBirthday));

    [Fact] public void JungleboyDog() =>
        Null(LookupJungleboy(FieldDog));

    [Fact] public void JungleboyUnknownProp() =>
        Null(LookupJungleboy(UnusedField));

    [Fact]
    public void JungleboyChildren() =>
        NotNull(LookupJungleboy(FieldChildren));

    [Fact]
    public void JungleboyChildrenIsListOfPropL() =>
        IsAssignableFrom<IEnumerable<IPropertyLookup>>(LookupJungleboy(FieldChildren));



    [Fact] public void JohnDoeName() =>
        Equal(JohnDoe.Name, LookupJohnDoe(FieldName));

    [Fact] public void JohnDoeNameCaseInsensitive() =>
        Equal(JohnDoe.Name, LookupJohnDoe(FieldName.ToUpper()));

    [Fact] public void JohnDoeBirthday() =>
        Equal(JohnDoe.Birthday, LookupJohnDoe(FieldBirthday));

    [Fact] public void JohnDoeDog() =>
        Equal(JohnDoe.Dog, LookupJohnDoe(FieldDog));
    [Fact] public void JohnDoeUnknownProp() =>
        Null(LookupJohnDoe(UnusedField));

    [Fact] public void JohnDoeChildren() =>
        Null(LookupJohnDoe(FieldChildren));

}