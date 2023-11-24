using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;


namespace ToSic.Eav.Data;

/// <summary>
/// A strongly typed object which gets its data from an entity, and also logs what it does.
/// </summary>
[PublicApi_Stable_ForUseInYourCode]
public abstract class EntityBasedWithLog: EntityBasedType, IHasLog
{
    /// <summary>
    /// An entity based type which also logs what it does
    /// </summary>
    /// <param name="entity">entity which will be the foundation of this type</param>
    /// <param name="parentLog">parent log to chain</param>
    /// <param name="logName">Name for the logger</param>
    protected EntityBasedWithLog(IEntity entity, ILog parentLog, string logName) : base(entity) => Log = new Log(logName, parentLog);

    /// <summary>
    /// An entity based type which also logs what it does
    /// </summary>
    /// <param name="entity">entity which will be the foundation of this type</param>
    /// <param name="languageCodes">languages to prefer in value lookups</param>
    /// <param name="parentLog">parent log to chain</param>
    /// <param name="logName">Name for the logger</param>
    protected EntityBasedWithLog(IEntity entity, string[] languageCodes, ILog parentLog, string logName) : base(entity, languageCodes) => Log = new Log(logName, parentLog);

    /// <summary>
    /// An entity based type which also logs what it does
    /// </summary>
    /// <param name="entity">entity which will be the foundation of this type</param>
    /// <param name="languageCode">language to prefer in value lookups</param>
    /// <param name="parentLog">parent log to chain</param>
    /// <param name="logName">Name for the logger</param>
    protected EntityBasedWithLog(IEntity entity, string languageCode, ILog parentLog, string logName) : base(entity, languageCode) => Log = new Log(logName, parentLog);

    /// <inheritdoc/>
    public ILog Log { get; }

}