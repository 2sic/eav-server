using ToSic.Eav.Data;

namespace ToSic.Eav.Models.TestData;

public class TestModelNullCapable : IModelFromEntity, IModelSetup<IEntity>
{

    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        return true;
    }

}

public class TestModelNullUnCapable : IModelFromEntity, IModelSetup<IEntity>
{

    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        return false;
    }

}