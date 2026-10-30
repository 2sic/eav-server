namespace ToSic.Eav.Serialization.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class SerializerExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IJsonDataReader SetApp(this IJsonDataReader serializer, IAppReader appReader) // SerializerBase
    {
        var l = (serializer as IHasLog)?.Log.Fn<IJsonDataReader>();
        serializer.Initialize(new(appReader));
        return l.Return(serializer);
    }

}