using System.Diagnostics;
using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Options;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.Persistence.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Internal.XmlList;

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

    //private int _appId;

    /// <summary>
    /// The xml document to imported.
    /// </summary>
    public XDocument Document { get; private set; }

    /// <summary>
    /// The elements of the xml document.
    /// </summary>
    public IEnumerable<XElement> DocumentElements { get; private set; }

    private string _docLangPrimary;

    private IList<string> _languages;

    protected bool ResolveLinks;

    private ImportDeleteUnmentionedItems _deleteSetting;




    /// <summary>
    /// The entities created from the document. They will be saved to the repository.
    /// </summary>
    public List<Entity> ImportEntities { get; } = [];

    private Entity GetImportEntity(Guid entityGuid)
    {
        var l = Log.Fn<Entity>();
        var result = ImportEntities.FirstOrDefault(entity => entity.EntityGuid == entityGuid);
        return l.Return(result, result == null ? "null" : $"Will modify entity from existing import list {entityGuid}");
    }


    private int _appendEntityCount = 0;

    private Entity AppendEntity(Guid entityGuid, IDictionary<string, IAttribute> values)
    {
        var l = Log.Fn<Entity>();
        if (_appendEntityCount++ < 100)
            l.A($"Add entity to import list {entityGuid}");
        if (_appendEntityCount == 100)
            l.A("Add entity: will stop listing each one...");
        if (_appendEntityCount % 100 == 0)
            l.A("Add entity: Current count:" + _appendEntityCount);
        var entity = builder.Entity.Create(appId: AppReader.AppId, guid: entityGuid, contentType: ContentType,
            attributes: builder.Attribute.Create(values));
        ImportEntities.Add(entity);
        return l.Return(entity);
    }

    /// <summary>
    /// Errors found while importing the document to memory.
    /// </summary>
    public ImportErrorLog ErrorLog { get; set; }


    private List<Guid> GetCreatedEntityGuids()
        => ImportEntities.Select(entity => entity.EntityGuid != Guid.Empty ? entity.EntityGuid : Guid.NewGuid()).ToList();


    /// <summary>
    /// Get the attribute names in the xml document.
    /// </summary>
    public IEnumerable<string> Info_AttributeNamesInDocument => DocumentElements.SelectMany(element => element.Elements())
        .GroupBy(attribute => attribute.Name.LocalName)
        .Select(group => group.Key)
        .Where(name => name != XmlConstants.EntityGuid && name != XmlConstants.EntityLanguage)
        .ToList();


    private IEntity FindInExisting(Guid guid)
        => ExistingEntities.FirstOrDefault(e => e.EntityGuid == guid);



}