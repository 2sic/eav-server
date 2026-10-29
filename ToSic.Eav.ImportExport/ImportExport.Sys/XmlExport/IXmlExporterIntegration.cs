using ToSic.Eav.Context;

namespace ToSic.Eav.ImportExport.Sys.XmlExport;

public interface IXmlExporterIntegration
{
    /// <summary>
    /// Post context init the caller must be able to init Adam, which is not part of this project, so we're handling it as a callback
    /// </summary>
    /// <param name="appContext"></param>
    void Setup(IContextOfApp appContext);

    public List<int> ReferencedFileIds { get; }
    public List<int> ReferencedFolderIds { get; }

    public string ResolveFolderId(int folderId);
    public TenantFileItem ResolveFile(int fileId);
    void AddFilesToExportQueue();
    void AddFileAndFolderToQueue(int fileNum);

}