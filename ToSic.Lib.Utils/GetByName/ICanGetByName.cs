namespace ToSic.Lib.GetByName;

/// <summary>
/// General property to get any kind of value by name only.
/// Important for things such as DynamicEntity, DynamicStack etc.
/// when passed around and must be accessed to get a value. 
/// </summary>
[PrivateApi("Very internal functionality, could change at any time")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICanGetByName
{
    object? Get(string name);
}