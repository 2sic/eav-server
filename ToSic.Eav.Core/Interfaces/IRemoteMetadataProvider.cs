namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// This interface allows objects to provid metadata from "remote" systems
    /// meaning from apps / sources which the original source doesn't know about
    /// </summary>
    public interface IRemoteMetadataProvider
    {
        IMetadataProvider OfZoneAndApp(int zoneId, int appId);

        IMetadataProvider OfApp(int appId);
    }
}
