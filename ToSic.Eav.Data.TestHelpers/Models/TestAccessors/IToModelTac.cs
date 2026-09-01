using ToSic.Eav.Data;

namespace ToSic.Eav.Models;

/// <summary>
/// Interface to describe a helper which calls ToModel.
/// It's needed so we can test things using both the static ToModel and the ToModelInternal() method.
/// </summary>
public interface IToModelTac
{
    public TModel? ToModelTac<TModel>(
        IEntity? entity,
        ToModelOptions? options = default
    ) where TModel : class, IModelFromEntity;
    
    public object? ToModel(IEntity entity, Type type, string? typeName);
}