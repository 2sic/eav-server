namespace ToSic.Eav.Context;

[PrivateApi]
public interface IRole
{
    /// <summary>
    /// Role Id as int. Works in DNN and Oqtane
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Role name.
    /// </summary>
    string Name { get; }
}