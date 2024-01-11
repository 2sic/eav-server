namespace ToSic.Eav.Serialization.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class SerializerExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static T SetApp<T>(this T serializer, IAppState appState) where T : SerializerBase
    {
        var l = serializer.Log.Fn<T>();
        serializer.Initialize(appState);
        return l.Return(serializer);
    }

}