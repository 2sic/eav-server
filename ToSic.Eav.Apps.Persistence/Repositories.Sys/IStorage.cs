﻿using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Persistence.Sys.Logging;


namespace ToSic.Eav.Repositories.Sys;

/// <summary>
/// This interface should ensure that storage layers can be swapped out as is necessary
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IStorage: IHasLog
{
    #region Transaction Support

    /// <summary>
    /// Perform an action wrapped as a transaction.
    /// This will cause the whole action to succeed, or to fail together. 
    /// </summary>
    /// <param name="action"></param>
    void DoInTransaction(Action action);

    #endregion

    #region Cache Refreshing

    /// <summary>
    /// Do an action, but wait with invalidating the system cache till the action is complete
    /// By default (if not using this command) each save will invalidate the cache
    /// </summary>
    /// <param name="action"></param>
    void DoWithDelayedCacheInvalidation(Action action);

    #endregion

    #region Versioning QUeue

    void DoWhileQueuingVersioning(Action action);

    #endregion

    #region RelationshipQueue

    void DoWhileQueueingRelationships(Action action);

    #endregion


    #region Loader

    /// <summary>
    /// This provides a loader to retrieve fully typed / configured app-packages for in-memory use, caching etc.
    /// </summary>
    IAppsAndZonesLoaderWithRaw Loader { get; }
    #endregion


    #region Logging (old / import)
    List<Message> ImportLogToBeRefactored { get; }
    #endregion


    #region Relationship Import / Queue

    //void DoWhileQueueingRelationships(Action action);

    #endregion

    #region Entities

    List<int> Save(List<IEntity> entities, SaveOptions saveOptions);

    #endregion


    #region ContentType Commands

    void Save(List<IContentType> contentTypes, SaveOptions saveOptions);

    #endregion

    int? ParentAppId { get; }
    int GetParentAppId(string parentAppGuid, int parentAppId);

    int CreateApp(string guidName, int? inheritAppId = null);
}