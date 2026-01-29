using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests.TestData;

public class TestModelNullCapable : IModelSetup<IEntity>
{

    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        return true;
    }

}

public class TestModelNullUnCapable : IModelSetup<IEntity>
{

    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        return false;
    }

}