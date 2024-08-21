namespace ToSic.Eav.Data;

/// <summary>
/// This marks add-on information for things which can be decorated.
/// This allows taking something (like an <see cref="IEntity"/> and adding additional information for later processing
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("just fyi")]
public interface IDecorator<T>: IDecorator;