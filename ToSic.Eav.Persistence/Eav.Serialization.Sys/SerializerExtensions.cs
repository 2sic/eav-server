using ToSic.Eav.ImportExport.Json.Sys;

namespace ToSic.Eav.Serialization.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class SerializerExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static JsonSerializer SetApp(this JsonSerializer serializer, IAppReader appReader)
    {
        var l = serializer.Log.Fn<JsonSerializer>();
        serializer.Initialize(new(appReader));
        return l.Return(serializer);
    }
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IJsonDataReader SetApp(this IJsonDataReader serializer, IAppReader appReader) // SerializerBase
    {
        var l = (serializer as IHasLog)?.Log.Fn<IJsonDataReader>();
        serializer.Initialize(new(appReader));
        return l.Return(serializer);
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IJsonDataWriter SetApp(this IJsonDataWriter serializer, IAppReader appReader) // SerializerBase
    {
        var l = (serializer as IHasLog)?.Log.Fn<IJsonDataWriter>();
        serializer.Initialize(new(appReader));
        return l.Return(serializer);
    }

}