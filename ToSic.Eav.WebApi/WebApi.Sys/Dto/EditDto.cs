﻿using ToSic.Eav.Apps.Sys;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Sys.Dto;

public record EditDto
{
    /// <summary>
    /// The items in the load/save package.
    /// </summary>
    public required List<BundleWithHeader<JsonEntity>> Items { get; init; }

    /// <summary>
    /// Content-Type information for the form
    /// </summary>
    public List<JsonContentType> ContentTypes { get; init; } = null!;

    /// <summary>
    /// Relevant input-types and their default labels & configuration
    /// </summary>
    public List<InputTypeInfo> InputTypes { get; init; } = null!;

    /// <summary>
    /// If this set is to be published - default true
    /// </summary>
    public bool IsPublished = true;

    /// <summary>
    /// If this set is to not-published, whether it should branch, leaving the original published
    /// </summary>
    public bool DraftShouldBranch = false;

    /// <summary>
    /// Experimental additional data for configuration
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<JsonEntity> ContentTypeItems { get; init; } = null!;

    /// <summary>
    /// The UI Context information
    /// </summary>
    public ContextDto Context { get; init; } = null!;

    /// <summary>
    /// Contain pre-fetched data to reduce callbacks just to look up data which obviously will be needed
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EditPrefetchDto? Prefetch { get; set; }

    /// <summary>
    /// Contains additional settings such as default GPS coordinates, content-types needed by pickers etc.
    /// </summary>
    /// <remarks>Added ca. v15</remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EditSettingsDto Settings { get; init; } = null!;

    /// <summary>
    /// WIP tell the UI what features are required v18.02...
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]> RequiredFeatures { get; init; } = null!;
}

public class EditSettingsDto
{
    public IDictionary<string, object>? Values { get; set; }
    public List<JsonEntity>? Entities { get; set; }

    /// <summary>
    /// ContentTypes which are used for Pickers to determine names etc.
    /// </summary>
    /// <remarks>
    /// Added v17, optimized with Title in v18.02
    /// </remarks>
    public List<JsonContentTypeWithTitleWip>? ContentTypes { get; set; }
}