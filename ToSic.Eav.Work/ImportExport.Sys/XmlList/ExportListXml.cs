﻿using System.Xml.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.ImportExport.Sys.XmlList;

/// <summary>
/// For exporting a content-type into xml, either just the schema or with data
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportListXml(ExportImportValueConversion valueConverter)
    : ServiceBase("App.LstExp", connect: [valueConverter])
{
    #region Dependency Injection and Init

    protected ExportImportValueConversion ValueConverter { get; } = valueConverter;

    public ExportListXml Init(IAppReader appReader, string typeName)
        => Init(appReader, appReader.GetContentType(typeName));

    public ExportListXml Init(IAppReader appReader, IContentType contentType)
    {
        AppReader = appReader;
        ContentType = contentType;
        return this;
    }

    private readonly XmlBuilder _xBuilder = new();
    private IAppReader AppReader { get; set; } = null!;
    public IContentType ContentType { get; set; } = null!;

    #endregion




    /// <summary>
    /// Create a blank xml scheme for data of one content-type
    /// </summary>
    /// <returns>A string containing the blank xml scheme</returns>
    public string? EmptyListTemplate()
    {
        var l = Log.Fn<string?>("export schema xml");
        if (ContentType == null) 
            return null;

        // build two empty nodes for easier filling in by the user
        var firstRow = _xBuilder.BuildEntity("", "", ContentType.Name);
        var secondRow = _xBuilder.BuildEntity("", "", ContentType.Name);
        var rootNode = _xBuilder.BuildDocumentWithRoot(firstRow, secondRow);

        var attributes = ContentType.Attributes;
        foreach (var attribute in attributes)
        {
            firstRow.Append(attribute.Name, "");
            secondRow.Append(attribute.Name, "");  
        }

        var result = rootNode.Document?.ToString();
        return l.Return(result);
    }

    /// <summary>
    /// Serialize data to an xml string.
    /// </summary>
    /// <param name="languageSelected">Language of the data to be serialized (null for all languages)</param>
    /// <param name="languageFallback">Language fallback of the system</param>
    /// <param name="sysLanguages">Languages supported of the system</param>
    /// <param name="exportLanguageReference">How value references to other languages are handled</param>
    /// <param name="resolveLinks">How value references to files and pages are handled</param>
    /// <param name="selectedIds">array of IDs to export only these</param>
    /// <returns>A string containing the xml data</returns>
    public string? GenerateXml(string languageSelected, string languageFallback, string[] sysLanguages, ExportLanguageResolution exportLanguageReference, bool resolveLinks, int[] selectedIds)
    {
        if (ContentType == null)
            return null;

        Log.A($"start export lang selected:{languageSelected} with fallback:{languageFallback} and type:{ContentType.Name}");

        #region build languages-list, but this must still be case-sensitive
        // note: reason for case-sensitive is that older imports need the en-US or will fail
        // newer imports would work correctly, but we want to be sure we can still re-import in older eav-systems
        var languages = new List<string>();
        if (!string.IsNullOrEmpty(languageSelected))// only selected language
            languages.Add(languageSelected);    
        else if (sysLanguages.Any())
            languages.AddRange(sysLanguages);// Export all languages
        else
            languages.Add(string.Empty); // default
        #endregion

        // neutralize languages AFTER creating the languages list, as that may not be lower-invariant!
        languageFallback = languageFallback.ToLowerInvariant();
        sysLanguages = sysLanguages
            .Select(l => l.ToLowerInvariant())
            .ToArray();

        var documentRoot = _xBuilder.BuildDocumentWithRoot();

        // Query all entities, or just the ones with specified IDs
        var entities = AppReader.GetListPublished().OfType(ContentType);
        if (selectedIds is { Length: > 0 })
            entities = entities.Where(e => selectedIds.Contains(e.EntityId));
        var entList = entities.ToList();

        // Get the attribute definitions
        var attribsOfType = ContentType.Attributes.ToListOpt();
        Log.A($"will export {entList.Count} entities X {attribsOfType.Count} attribs");

        foreach (var entity in entList)
        foreach (var language in languages)
        {
            var xmlEntity = _xBuilder.BuildEntity(entity.EntityGuid, language, ContentType.Name);
            var langLow = language.ToLowerInvariant();
            documentRoot.Add(xmlEntity);

            foreach (var attribute in attribsOfType)
            {
                string? value;
                if (attribute.Type == ValueTypes.Entity) // Special, handle separately
                    value = entity.Attributes[attribute.Name].Values.FirstOrDefault()?.Serialized;
                else
                    value = exportLanguageReference == ExportLanguageResolution.Resolve
                        ? ValueConverter.ValueWithFullFallback(entity, attribute, langLow, languageFallback, resolveLinks)
                        : ValueConverter.ValueOrLookupCode(entity, attribute, langLow, languageFallback,
                            sysLanguages, languages.Count > 1, resolveLinks);

                xmlEntity.Append(attribute.Name, value);
            }
        }

        return documentRoot.Document?.ToString();
    }

}

internal static class QuickExtensions
{
    //internal static int IndexOf<T>(this IEnumerable<T> list, T item) 
    //    => list.TakeWhile(i => !i.Equals(item)).Count();

    /// <summary>
    /// Apend an element to this.
    /// </summary>
    internal static void Append(this XElement element, XName name, object? value)
        => element.Add(new XElement(name, value));
        
}