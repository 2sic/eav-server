namespace ToSic.Eav.Persistence.Sys.AppState;

[PrivateApi("don't publish this - too internal, special, complicated")]
public enum AppStateLoadSequence
{
    Start,
    // ReSharper disable once UnusedMember.Global
    AppPathInit,
    MetadataInit,
    ContentTypeLoad,
    ItemLoad
}