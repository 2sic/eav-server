using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Shared base tests for ToModel actions - with virtual methods, so it can also be overriden for ToModelInternal()
/// </summary>
public abstract class ToModelTestsBase(IMockMetadataForGenerator generator, bool useInternal)
{
    #region Test Setup Helpers to create models either using the internal ToModelInternal or the public ToModel

    protected TModel GetModel<TModel>(string? typeName)
        where TModel : class, IModelFromEntity
    {
        var options = new ToModelOptions { TypeName = typeName };
        return !useInternal
            ? generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: options)!
            : generator.CreateMetadataForDecorator().ToModelInternal<TModel>(options: options)!;
    }

    protected TModel GetModel<TModel>()
        where TModel : class, IModelFromEntity
        => !useInternal
            ? generator.CreateMetadataForDecorator().ToModelTac<TModel>()!
            : generator.CreateMetadataForDecorator().ToModelInternal<TModel>(new())!;

    protected TModel GetModelSkipTypeCheck<TModel>()
        where TModel : class, IModelFromEntity
        => !useInternal
            ? generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: ToModelOptions.DisableTypeNameCheck)!
            : generator.CreateMetadataForDecorator().ToModelInternal<TModel>(ToModelOptions.DisableTypeNameCheck)!;

    #endregion
}