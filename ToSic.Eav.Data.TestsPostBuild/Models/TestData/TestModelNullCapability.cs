using ToSic.Eav.Data;

namespace ToSic.Eav.Models.TestData;

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