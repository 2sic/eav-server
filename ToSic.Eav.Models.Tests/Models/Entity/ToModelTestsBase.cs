using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Shared base tests for ToModel actions - with virtual methods, so it can also be overriden for ToModelInternal()
/// </summary>
public abstract class ToModelTestsBase(IMockMetadataForGenerator generator, IToModelTac toModelTac)
{
    #region Test Setup Helpers to create models either using the internal ToModelInternal or the public ToModel

    protected TModel? GetModel<TModel>()
        where TModel : class, IModelFromEntity
        => toModelTac.ToModelTac<TModel>(generator.CreateMetadataForDecorator());

    protected TModel? GetModelSkipTypeCheck<TModel>()
        where TModel : class, IModelFromEntity
        => toModelTac.ToModelTac<TModel>(generator.CreateMetadataForDecorator(), ToModelOptions.DisableTypeNameCheck);
    
    #endregion
}
