using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.Entity;

/// <summary>
/// Shared base tests for ToModel actions - with virtual methods, so it can also be overriden for ToModelInternal()
/// </summary>
public abstract class ToModelTestsBase(IMockMetadataForGenerator generator, bool useInternal) : IHasMockMetadataForGenerator
{
    public bool UseInternal => useInternal;
    public IMockMetadataForGenerator Generator => generator;

    #region Test Setup Helpers to create models either using the internal ToModelInternal or the public ToModel

    protected TModel GetModel<TModel>()
        where TModel : class, IModelFromEntity
        => !UseInternal
            ? Generator.CreateMetadataForDecorator().ToModelTac<TModel>()!
            : Generator.CreateMetadataForDecorator().ToModelInternal<TModel>(new())!;

    
    protected TModel GetModelSkipTypeCheck<TModel>()
        where TModel : class, IModelFromEntity
        => !UseInternal
            ? Generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: ToModelOptions.DisableTypeNameCheck)!
            : Generator.CreateMetadataForDecorator().ToModelInternal<TModel>(ToModelOptions.DisableTypeNameCheck)!;

    #endregion
}

public interface IHasMockMetadataForGenerator
{
    IMockMetadataForGenerator Generator { get; }
    bool UseInternal { get; }
}

public static class IHasMockMetadataForGeneratorExtension
{
    public static TModel GetModel<TModel>(this IHasMockMetadataForGenerator hasMockMetadata, string? typeName)
        where TModel : class, IModelFromEntity
    {
        var options = new ToModelOptions { TypeName = typeName };
        return !hasMockMetadata.UseInternal
            ? hasMockMetadata.Generator.CreateMetadataForDecorator().ToModelTac<TModel>(options: options)!
            : hasMockMetadata.Generator.CreateMetadataForDecorator().ToModelInternalTac<TModel>(options: options)!;
    }

    
    public static object GetModel(this IHasMockMetadataForGenerator hasMockMetadata, Type type, bool callInternal, string? typeName)
    {
        var options = new ToModelOptions { TypeName = typeName };
        
        return !callInternal
            ? hasMockMetadata.Generator.CreateMetadataForDecorator().ToModelTac(type, options: options)!
            : hasMockMetadata.Generator.CreateMetadataForDecorator().ToModelInternalTac(type, options: options)!;
    }

}