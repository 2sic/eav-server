using ToSic.Eav.ImportExport.Json.Sys;

namespace ToSic.Eav.Serialization.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class SerializerExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static T SetApp<T>(this T serializer, IAppReader appReader) where T : JsonSerializer // SerializerBase
    {
        var l = serializer.Log.Fn<T>();
        serializer.Initialize(new JsonSerializer.DeserializerOptions(appReader));
        return l.Return(serializer);
    }

}