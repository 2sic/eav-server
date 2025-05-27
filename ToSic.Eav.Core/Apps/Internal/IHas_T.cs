//namespace ToSic.Eav.Apps.Internal;

///// <summary>
///// Experimental interface to mark objects which have a value.
///// ATM to better handle App-State-Like objects without showing the underlying data.
///// </summary>
///// <typeparam name="T"></typeparam>
//[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
//[ShowApiWhenReleased(ShowApiMode.Never)]
//public interface IHas<out T> where T : class
//{
//    T Value { get; }
//}