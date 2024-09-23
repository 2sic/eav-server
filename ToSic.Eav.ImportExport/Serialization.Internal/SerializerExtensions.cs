using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Serialization.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class SerializerExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static T SetApp<T>(this T serializer, IAppReader appReader) where T : SerializerBase
    {
        var l = serializer.Log.Fn<T>();
        serializer.Initialize(appReader);
        return l.Return(serializer);
    }

}