using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests.TestData;

public class TestModelNullCapable : IWrapperSetup<IEntity>
{

    bool IWrapperSetup<IEntity>.SetupContents(IEntity? source)
    {
        return true;
    }

}

public class TestModelNullUnCapable : IWrapperSetup<IEntity>
{

    bool IWrapperSetup<IEntity>.SetupContents(IEntity? source)
    {
        return false;
    }

}