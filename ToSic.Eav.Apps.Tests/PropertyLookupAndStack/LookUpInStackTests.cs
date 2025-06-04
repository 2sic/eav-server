using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;


public class LookUpInStackTests: PropLookupStackBase
{
    [Fact] public void BasicDotNotation() =>
        Equal(ChildJb1.Name, GetLookup().GetTac($"{FieldChildren}.{FieldName}"));

    [Fact] public void BasicColonNotation() =>
        Equal(ChildJb1.Name, GetLookup().GetTac($"{FieldChildren}:{FieldName}"));

    private LookUpInStack GetLookup()
    {
        var lookup = new LookUpInStack("Settings", FirstJungleboy, PropReqSpecs.EmptyDimensions);
        return lookup;
    }
}