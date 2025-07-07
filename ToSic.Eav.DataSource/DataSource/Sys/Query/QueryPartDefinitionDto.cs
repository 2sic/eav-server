﻿using System.Text.Json.Nodes;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.DataSource.Sys.Query;
// TODO: this is the old untyped implementation - should be replaced with a clean Dto version
// Note that the DTO already exists in another file...

// not really easy, because it appears that the EntityGuid is used as a string to send back infos from the UI
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class QueryPartDefExtTemp
{
    public static Dictionary<string, object?> AsDictionary(this QueryPartDefinition part)
    {
        var attributes = part.Entity.AsDictionary();

        attributes[nameof(QueryPartDefinition.VisualDesignerData)] = JsonNode
            .Parse(part.VisualDesignerData, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions)!
            .AsObject();

        // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
        attributes[nameof(QueryPartDefinition.PartAssemblyAndType)] = part.DataSourceTypeIdentifier;

        return attributes;
    }

}