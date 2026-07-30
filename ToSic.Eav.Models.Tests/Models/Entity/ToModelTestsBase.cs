using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Shared base tests for ToModel actions - with virtual methods, so it can also be overriden for ToModelInternal()
/// </summary>
public abstract class ToModelTestsBase(MockDataGenerator generator, bool useInternal)
{
    #region Test Setup Helpers to create models either using the internal ToModelInternal or the public ToModel

    protected virtual TModel? GetModelNoParams<TModel>()
        where TModel : class, IModelFromEntity
        => !useInternal
            ? generator.CreateMetadataForDecorator().ToModelTac<TModel>()!
            : generator.CreateMetadataForDecorator().ToModelInternal<TModel>(new())!;

    protected virtual TModel? GetModelSkipTypeCheck<TModel>()
        where TModel : class, IModelFromEntity
        => !useInternal
            ? generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: ToModelOptions.DisableTypeNameCheck)!
            : generator.CreateMetadataForDecorator().ToModelInternal<TModel>(ToModelOptions.DisableTypeNameCheck)!;

    #endregion
}