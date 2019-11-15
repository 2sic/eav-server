using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    [PrivateApi("don't publish this - too internal, special, complicated")]
    public enum AppStateLoadSequence
    {
        Start,
        MetadataInit,
        ContentTypeLoad,
        ItemLoad
    }
}
