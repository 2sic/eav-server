using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;

namespace ToSic.Eav.Serialization;

/// <summary>
/// Marks objects that can serialize EAV data like Entities or Content-Types. <br/>
/// </summary>
[PrivateApi("not ready for publishing")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDataSerializer
{
    void Initialize(IAppReader appState);

    /// <summary>
    /// Serialize an entity to a string
    /// </summary>
    /// <param name="entityId">ID - the serializer will look for this in the app</param>
    /// <returns>a serialized entity as string</returns>
    string Serialize(int entityId);

    /// <summary>
    /// Batch-serialize a bunch of entities
    /// </summary>
    /// <param name="entities">list of the entities</param>
    /// <returns>A string-dictionary containing all the serialized entities</returns>
    Dictionary<int, string> Serialize(List<int> entities);

    /// <summary>
    /// Serialize an entity to a string
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    string Serialize(IEntity entity);

    /// <summary>
    /// Batch-serialize a bunch of entities
    /// </summary>
    /// <param name="entities">List of entities to serialize</param>
    /// <returns>A string-dictionary containing all the serialized entities</returns>
    Dictionary<int, string> Serialize(List<IEntity> entities);


}