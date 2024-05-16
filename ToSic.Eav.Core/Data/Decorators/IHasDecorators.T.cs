namespace ToSic.Eav.Data;

/// <summary>
/// This marks objects which carry additional decorator information
/// </summary>
/// <typeparam name="T"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice("Marks objects which have decorator information")]
public interface IHasDecorators<T>
{
    List<IDecorator<T>> Decorators { get; }
}