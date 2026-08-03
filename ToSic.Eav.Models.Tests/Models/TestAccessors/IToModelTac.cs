using ToSic.Eav.Data;

namespace ToSic.Eav.Models;

public interface IToModelTac
{
    internal TModel? ToModelTac<TModel>(
        IEntity? entity,
        ToModelOptions? options = default
    ) where TModel : class, IModelFromEntity;
    
    internal object? ToModel(IEntity entity, Type type, string? typeName);
}