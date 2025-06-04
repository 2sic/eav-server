namespace ToSic.Eav.Serialization.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class SerializerExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static T SetApp<T>(this T serializer, IAppReader appReader) where T : SerializerBase
    {
        var l = serializer.Log.Fn<T>();
        serializer.Initialize(appReader);
        return l.Return(serializer);
    }

}