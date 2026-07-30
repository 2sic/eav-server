using ToSic.Eav.Data;

namespace ToSic.Eav.Models.NullHandlingTests;

public abstract class NullHandlingBase
{
    #region Test Data and test exceptions

    protected class MockModelNullDataOk : IModelFromEntity, IModelSetup<IEntity>
    {
        bool IModelSetup<IEntity>.SetupModel(IEntity? source) => true;
    }

    
    protected class MockModelNullDataRejected : IModelFromEntity, IModelSetup<IEntity>
    {
        bool IModelSetup<IEntity>.SetupModel(IEntity? source) => false;
    }

    
    protected class MockModelNullDataThrow : IModelFromEntity, IModelSetup<IEntity>
    {
        bool IModelSetup<IEntity>.SetupModel(IEntity? source) => throw new CustomException();
    }

    protected class CustomException : Exception;

    #endregion    
}