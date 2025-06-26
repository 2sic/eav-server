using System.Diagnostics;
using System.Xml.Linq;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.Persistence.Sys.Logging;


namespace ToSic.Eav.ImportExport.Sys.XmlList;

partial class ImportListXml
{
    #region Timing / Debuging infos

    /// <summary>
    /// Helper to measure time used for stuff
    /// </summary>
    public Stopwatch Timer { get; set; } = new();

    public long TimeForMemorySetup;
    public long TimeForDbImport;
    #endregion

    /// <summary>
    /// The elements of the xml document.
    /// </summary>
    public IEnumerable<XElement> DocumentElements { get; private set; } = null!;

    //private IList<string> _languages;

    private ImportConfiguration ImportConfig { get; set; } = null!;

    private record ImportConfiguration
    {
        public required string DocLangPrimary { get; init; }
        public required IList<string> Languages { get; init; }
    }

    protected bool ResolveLinks;

    private ImportDeleteUnmentionedItems _deleteSetting;




    /// <summary>
    /// The entities created from the document. They will be saved to the repository.
    /// </summary>
    public List<Entity> ImportEntities { get; } = [];

    private Entity? GetImportEntity(Guid entityGuid)
    {
        var l = Log.Fn<Entity?>();
        var result = ImportEntities.FirstOrDefault(entity => entity.EntityGuid == entityGuid);
        return l.Return(result, result == null ? "null" : $"Will modify entity from existing import list {entityGuid}");
    }


    private int _appendEntityCount = 0;

    private Entity AppendEntity(int appId, IContentType contentType, Guid entityGuid,
        IDictionary<string, IAttribute> values)
    {
        var l = Log.Fn<Entity>();
        if (_appendEntityCount++ < 100)
            l.A($"Add entity to import list {entityGuid}");
        if (_appendEntityCount == 100)
            l.A("Add entity: will stop listing each one...");
        if (_appendEntityCount % 100 == 0)
            l.A("Add entity: Current count:" + _appendEntityCount);
        var entity = builder.Entity.Create(appId: appId, guid: entityGuid, contentType: contentType,
            attributes: builder.Attribute.Create(values));
        ImportEntities.Add(entity);
        return l.Return(entity);
    }

    /// <summary>
    /// Errors found while importing the document to memory.
    /// </summary>
    public ImportErrorLog ErrorLog { get; set; } = null!;


    private List<Guid> GetCreatedEntityGuids(List<Entity> importEntities)
        => importEntities
            .Select(entity => entity.EntityGuid != Guid.Empty ? entity.EntityGuid : Guid.NewGuid())
            .ToList();


    ///// <summary>
    ///// Get the attribute names in the xml document.
    ///// </summary>
    //public IEnumerable<string> GetInfo_AttributeNamesInDocument(IEnumerable<XElement> xmlEntities) =>
    //    xmlEntities
    //        .SelectMany(element => element.Elements())
    //        .GroupBy(attribute => attribute.Name.LocalName)
    //        .Select(group => group.Key)
    //        .Where(name => name != XmlConstants.EntityGuid && name != XmlConstants.EntityLanguage)
    //        .ToList();


    //private IEntity? FindInExisting(Guid guid)
    //    => ExistingEntities.One(guid);
}