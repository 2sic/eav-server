namespace ToSic.Eav.Data.Sys.Entities;

/// <summary>
/// Experimental type for a thing which needs services to work, but has all the data from a Entity.
/// </summary>
[PrivateApi("WIP v15")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class EntityBasedService(string logName) : EntityBasedWithLog(null!, null, logName);
