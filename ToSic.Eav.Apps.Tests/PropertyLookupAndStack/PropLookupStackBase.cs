using ToSic.Eav.Data;
using ToSic.Eav.Data.PropertyStack.Sys;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;

public class PropLookupStackBase : PropertyLookupTestBase
{
    /// <summary>
    /// This will be the name of the stack.
    /// When using GetPath it should also be able to ignore this.
    /// </summary>
    protected const string StackName = "SettingsStack";

    protected readonly PropertyStack FirstJungleboy = new PropertyStack()
        .Init(StackName, Jungleboy.StackPart, JohnDoe.StackPart, JaneDoeWithChildren.StackPart);

    protected object FindInJbJd(string field) => GetResult(FirstJungleboy, field);
    protected object FindInJungleFirstPath(string fieldPath) => GetRequestPath(FirstJungleboy, fieldPath).Result;

    protected readonly PropertyStack FirstJohnDoe = new PropertyStack().Init(StackName, JohnDoe.StackPart, Jungleboy.StackPart);

    protected object FindInJohnDoe(string field) => GetResult(FirstJohnDoe, field);

}