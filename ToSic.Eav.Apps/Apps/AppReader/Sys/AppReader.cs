﻿using System.Collections.Immutable;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Apps.AppReader.Sys;

public class AppReader() : ServiceBase("App.Reader"), IAppReader
{
    internal AppReader Init(AppState appState, ILog? parentLog)
    {
        _appState = appState;
        this.LinkLog(parentLog);
        return this;
    }

    private AppState _appState = null!;

    /// <summary>
    /// internal, but in some cases it will be given out by hidden extension methods
    /// </summary>
    public IAppStateCache AppState => _appState;

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public IAppSpecs Specs
        => field ??= new AppSpecs(_appState);

    #region Identity

    public int ZoneId => _appState.ZoneId;

    public int AppId => _appState.AppId;

    #endregion

    #region Health Check

    public bool IsHealthy => _appState.IsHealthy;

    public string HealthMessage => _appState.HealthMessage;

    #endregion

    #region Normal Data / Entities / List / Draft / Publish

    public IImmutableList<IEntity> List
        => _appState.List;

    public IEntity? GetDraft(IEntity? entity)
        => _appState.GetDraft(entity);

    public IEntity? GetPublished(IEntity? entity)
        => _appState.GetPublished(entity);

    #endregion

    #region Content Types

    public IEnumerable<IContentType> ContentTypes
        => _appState.ContentTypes;

    public IContentType? TryGetContentType(string name)
        => _appState.TryGetContentType(name);

    public IContentType GetContentType(string name)
        => _appState.TryGetContentType(name)
        ?? throw new ArgumentException($@"Can't find content type with name '{name}'", nameof(name));

    public IContentType? GetContentTypeOptional(int contentTypeId)
        => _appState.TryGetContentType(contentTypeId);
    public IContentType GetContentTypeRequired(int contentTypeId)
        => _appState.TryGetContentType(contentTypeId)
           ?? throw new ArgumentException($@"Can't find content type with name '{contentTypeId}'", nameof(contentTypeId));

    #endregion


    #region Metadata

    public IMetadataSource Metadata => _appState.MetadataSource;

    #endregion

}