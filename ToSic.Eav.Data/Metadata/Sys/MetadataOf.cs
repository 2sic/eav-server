﻿using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Entities.Sys.Sources;
using ToSic.Eav.Metadata.Targets;
using ToSic.Lib.Helpers;
using ToSic.Sys.Caching;

namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// Metadata of an item (a content-type or another entity). <br/>
/// It's usually on a <strong>Metadata</strong> property of things that can have metadata.
/// </summary>
/// <typeparam name="T">The type this metadata uses as a key - int, string, guid</typeparam>
/// <remarks>
/// * Since v15.04 fully #immutable
/// </remarks>
[PrivateApi] // changed 2020-12-09 v11.11 from [PublicApi_Stable_ForUseInYourCode] - as this is a kind of lazy-metadata, we should change it to that
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class MetadataOf<T> : IMetadataOf, IMetadataInternals, ITimestamped
{

    #region Constructors

    /// <summary>
    /// Constructor that can take both a direct App-Source and a deferred source.
    /// Note that both sources can be null!
    /// </summary>
    /// <param name="targetType"></param>
    /// <param name="key"></param>
    /// <param name="title">Title of the target we're describing - for further automating when using or creating more Metadata</param>
    /// <param name="items">A direct list of items to use as metadata - instead of lazy-loading from a source. If specified, auto-sync will be disabled.</param>
    /// <param name="appSource"></param>
    /// <param name="deferredSource"></param>
    public MetadataOf(int targetType, T key, string title, IEnumerable<IEntity>? items = default, IHasMetadataSourceAndExpiring? appSource = default, Func<IHasMetadataSourceAndExpiring>? deferredSource = default)
    {
        _targetType = targetType;
        Key = key;
        _metadataTitle = title;
        Source = new(items == null ? null : new ImmutableEntitiesSource(items.ToImmutableOpt()), appSource, deferredSource);
    }

    protected VariableSource<IHasMetadataSourceAndExpiring> Source { get; }

    #endregion

    #region Debug Code to re-activate if every something is hard to decipher

    //public string Debug()
    //{
    //    return 
    //        $"{nameof(_debugAllEntry)}: {_debugAllEntry}, " +
    //        $"{nameof(_loadAllInLock.PreLockCount)}: {_loadAllInLock.PreLockCount}, " +
    //        $"{nameof(_loadAllInLock.LockCount)}: {_loadAllInLock.LockCount}" +
    //        $"{nameof(_debugAllReturn)}: {_debugAllReturn}, " +
    //        $"{nameof(_debugLoadFromProvider)}: {_debugLoadFromProvider}, " +
    //        $"{nameof(_debugUse)}: {_debugUse}, " +
    //        $"{nameof(CacheTimestamp)}: {CacheTimestamp} , " +
    //        $"{nameof(_appMetadataSource)}: {_appMetadataSource != null}, " +
    //        $"{nameof(_metaSourceRemote)}: {_metaSourceRemote != null}, " +
    //        $"{nameof(_allEntities)}: {_allEntities != null}, " +
    //        $"{nameof(_metadataWithoutPermissions)}: {_metadataWithoutPermissions != null}, " +
    //        $"{nameof(_mdsGetOnce)}: {_mdsGetOnce.IsValueCreated}, ";
    //}

    //private int _debugAllEntry;
    //private int _debugLoadFromProvider;
    //private int _debugAllReturn;
    //private int _debugUse;

    #endregion

    /// <summary>
    /// Type-information of the thing we're describing. This is used to retrieve metadata from the correct sub-list of pre-indexed metadata
    /// </summary>
    private readonly int _targetType;

    /// <summary>
    /// The key which identifies the item we're enriching with metadata
    /// </summary>
    protected virtual T Key { get; }

    #region Target

    public ITarget Target => _target.Get(() => new Target(_targetType, _metadataTitle, Key))!;
    private readonly GetOnce<ITarget> _target = new();
    private readonly string _metadataTitle;

    #endregion

    #region Context for data to be created

    public IAppIdentity? Context(string type) => GetMetadataSource();

    public (int TargetType, ICollection<IEntity>? list, IHasMetadataSourceAndExpiring? appSource, Func<IHasMetadataSourceAndExpiring>? deferredSource) GetCloneSpecs() 
        => (_targetType, Source.SourceDirect?.List?.ToListOpt(), Source.SourceApp, Source.SourceDeferred);

    #endregion
}