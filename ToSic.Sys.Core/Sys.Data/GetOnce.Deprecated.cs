namespace ToSic.Sys.Data;

/// <summary>
/// Obsolete implementation of lazy-get, which is now replaced by LazyGet.
/// This class is kept for backward compatibility, but should be replaced in all code with LazyGet.
/// </summary>
/// <typeparam name="TValue"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
[Obsolete("Deprecated in v22, pls change all code which uses this to use LazyGet instead; #ToBeRemovedV24")]
public class GetOnce<TValue>: LazyGet<TValue>;