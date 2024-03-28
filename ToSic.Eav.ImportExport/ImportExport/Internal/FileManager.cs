using System.IO;
using ToSic.Eav.Helpers;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using static System.IO.Path;

namespace ToSic.Eav.ImportExport.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FileManager(LazySvc<IAppJsonService> appJsonService) : ServiceBase(EavLogs.Eav + ".FileMn")
{
    private const char Separator = ';';


    public FileManager SetFolder(string sourceFolder, string subFolder = null)
    {
        _sourceFolder = sourceFolder; // we need to preserve it as "AppRoot" to get app.json
        _root =
            string.IsNullOrEmpty(
                sourceFolder) // we need it as "root" for files for export, root is usually same or under "sourceFolder"
                ? string.Empty
                : string.IsNullOrEmpty(subFolder)
                    ? sourceFolder
                    : Combine(sourceFolder, subFolder);
        return this;
    }

    private string _sourceFolder;
    private string _root;

    /// <summary>
    /// Copy all files from SourceFolder to DestinationFolder (directly on the file system)
    /// </summary>
    /// <param name="destinationFolder"></param>
    /// <param name="overwriteFiles"></param>
    /// <param name="messages"></param>
    public void CopyAllFiles(string destinationFolder, bool overwriteFiles, List<Message> messages
    ) => Log.Do($"dest:{destinationFolder}, overwrite:{overwriteFiles}", () =>
    {
        var filteredFiles = GetAllTransferableFiles().ToList();
        Log.A($"copy files:{filteredFiles.Count}");
        foreach (var file in filteredFiles)
        {
            var relativeFilePath = file.Replace(_root, "").TrimPrefixSlash();
            var destinationFilePath = $"{destinationFolder}{DirectorySeparatorChar}{relativeFilePath}";
            Log.A($"relFilePath:{relativeFilePath},destFilePath:{destinationFilePath}");

            Directory.CreateDirectory(GetDirectoryName(destinationFilePath) ?? string.Empty);

            if (!File.Exists(destinationFilePath))
                File.Copy(file, destinationFilePath, overwriteFiles);
            else
            {
                var alreadyExistsMsg = "File '" + GetFileName(destinationFilePath) +
                                       "' not copied because it already exists";
                Log.A($"warning: {alreadyExistsMsg}");
                messages.Add(new(alreadyExistsMsg, Message.MessageTypes.Warning));
            }
        }
    });

    /// <summary>
    /// Get exclude search patterns from app.json
    /// </summary>
    /// <returns></returns>
    private List<string> ExcludeSearchPatterns => _excludeSearchPatterns.Get(() => appJsonService.Value.ExcludeSearchPatterns(_sourceFolder));
    private readonly GetOnce<List<string>> _excludeSearchPatterns = new();


    /// <summary>
    /// Gets all files from a folder and subfolder, which fit the import/export filter criteria
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllTransferableFiles(string searchPattern = "*.*") => Log.Func(l =>
    {
        var hardcodedExcludedFolders = GetHardcodedExcludedFolders();
        l.A($"Hardcoded excluded folders count: {hardcodedExcludedFolders.Count}");

        var pathToDotAppJson = appJsonService.Value.GetPathToDotAppJson(_sourceFolder);
        IEnumerable<string> files;

        if (File.Exists(pathToDotAppJson))
        {
            l.A($"Exclude files based on {pathToDotAppJson}");
            files = ExcludeFilesBasedOnExcludeInDotAppJson(_root, searchPattern);
        }
        else
        {
            l.A($"Can't find {pathToDotAppJson}, exclude files using old way");
            files = AllFiles(searchPattern);
        }

        l.A($"Process excluding files based on hardcoded exclusions");
        var filteredFiles = files
            .Where(file => !FileManager.IsFileInExcludedFolder(file, hardcodedExcludedFolders))
            .ToList();

        l.A($"Returning filtered files based on exclusion criteria");
        return (filteredFiles, "Done filtering files based on exclusion criteria");
    });

    private List<string> GetHardcodedExcludedFolders() => Log.Func(l =>
    {
        // add folder slashes to ensure the term is part of a folder, not within a file-name
        var exclAnyFolder = Settings.ExcludeFolders
            .Select(f => $"{DirectorySeparatorChar}{f}{DirectorySeparatorChar}")
            .ToList();

        var exclRootFolders = Settings.ExcludeRootFolders
            .Select(f => $"{Combine(_root, f)}{DirectorySeparatorChar}")
            .ToList();

        var result = exclAnyFolder.Union(exclRootFolders).ToList();
        return (result, "Ok, building list of hardcoded excluded folders");
    });

    private static bool IsFileInExcludedFolder(string filePath, IEnumerable<string> excludedFolders) 
        => excludedFolders.Any(ex => filePath.ToLowerInvariant().Contains(ex.ToLowerInvariant()));


    /// <summary>
    /// Gets all folders from a folder and subfolder, which fit the import/export filter criteria
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllTransferableFolders(string searchPattern = "*.*") => Log.Func(l =>
    {
        var pathToDotAppJson = appJsonService.Value.GetPathToDotAppJson(_sourceFolder);
        if (File.Exists(pathToDotAppJson)) // exclude files based on app.json from v14.08
            return (ExcludeFoldersBasedOnExcludeInDotAppJson(_root, searchPattern).ToList(), $"ok, exclude folders based on {pathToDotAppJson}");

        // old hardcoded way of excluding files
        l.A($"can't find {pathToDotAppJson}, exclude folders on old way");
        // add folder slashes to ensure the term is part of a folder, not within a file-name
        var exclAnyFolder = Settings.ExcludeFolders
            .Select(f => $"{DirectorySeparatorChar}{f}{DirectorySeparatorChar}")
            .ToList();
        var exclRootFolders = Settings.ExcludeRootFolders
            .Select(f => $"{Combine(_root, f)}{DirectorySeparatorChar}").ToList();
        var excFolders = exclAnyFolder.Union(exclRootFolders).ToList();
        Log.A($"hardcoded, excFolders:{excFolders.Count()}");

        return (AllFolders(searchPattern)
            .Where(f => !excFolders.Any(ex => f.ToLowerInvariant().Contains(ex.ToLowerInvariant())))
            .ToList(),$"ok, exclude folders on old way");
    });


    /// <summary>
    /// Get all files from a folder, not caring if it will be exported or not
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> AllFiles(string searchPattern = "*.*") =>
        Directory.Exists(_root)
            ? searchPattern.Split(Separator).SelectMany(s => Directory.EnumerateFiles(_root, s, SearchOption.AllDirectories)).ToList()
            : [];


    /// <summary>
    /// Get all folders from a folder, not caring if it will be exported or not
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> AllFolders(string searchPattern = "*.*") =>
        Directory.Exists(_root)
            ? searchPattern.Split(Separator).SelectMany(s => Directory.EnumerateDirectories(_root, s, SearchOption.AllDirectories)).ToList()
            : [];


    /// <summary>
    /// Exclude files based on 2sxc special exclude array in app.json
    /// </summary>
    /// <param name="root"></param>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    private IEnumerable<string> ExcludeFilesBasedOnExcludeInDotAppJson(string root, string searchPattern = "*.*") => Log.Func($"folderPath:{root}", l =>
    {
        // validate folderPath
        if (!Directory.Exists(root))
            return ([], $"warning, can't find root folder path '{root}'");

        // validate exclude search patterns
        if (!ExcludeSearchPatterns.Any())
        {
            var allFiles = searchPattern.Split(Separator)
                .SelectMany(s => Directory.EnumerateFiles(root, s, SearchOption.AllDirectories)).ToList();
            return (allFiles, $"warning, can't find 2sxc exclude in '{Constants.AppJson}'");
        }

        // prepare list of files to exclude using simple wildcard patterns
        l.A($"excludeSearchPatterns:{ExcludeSearchPatterns.Count}");

        // *** EXCLUDE FOLDERS & FILES
        var files = GetFilesRecursion(root, ExcludeSearchPatterns, null, searchPattern);

        return (files,"ok");
    });

    /// <summary>
    /// Exclude folders based on 2sxc special exclude array in app.json
    /// </summary>
    /// <param name="root"></param>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    private IEnumerable<string> ExcludeFoldersBasedOnExcludeInDotAppJson(string root, string searchPattern = "*.*") => Log.Func($"folderPath:{root}", l =>
    {
        // validate folderPath
        if (!Directory.Exists(root))
            return ([], $"warning, can't find folder path '{_sourceFolder}'");

        // validate exclude search patterns
        if (!ExcludeSearchPatterns.Any())
        {
            var allFolders = searchPattern.Split(Separator)
                .SelectMany(s => Directory.EnumerateDirectories(_root, s, SearchOption.AllDirectories)).ToList();
            return (allFolders, $"warning, can't find 2sxc exclude in '{Constants.AppJson}'");
        }

        // prepare list of files to exclude using simple wildcard patterns
        Log.A($"excludeSearchPatterns:{ExcludeSearchPatterns.Count}");

        // *** EXCLUDE FOLDERS & FILES
        var folders = GetFoldersRecursion(root, ExcludeSearchPatterns, null, searchPattern);

        return (folders,"ok");
    });

    private List<string> GetFilesRecursion(string folderPath, List<string> excludeSearchPatterns, List<string> allFiles = null, string searchPattern = "*.*")
    {
        if (allFiles == null) allFiles = ExcludeFiles(folderPath, excludeSearchPatterns, searchPattern).ToList();

        foreach (var folder in ExcludeFolders(folderPath, excludeSearchPatterns).ToList())
        {
            allFiles.AddRange(ExcludeFiles(folder, excludeSearchPatterns, searchPattern).ToList());
            GetFilesRecursion(folder, excludeSearchPatterns, allFiles);
        }

        return allFiles;
    }

    private List<string> GetFoldersRecursion(string folderPath, List<string> excludeSearchPatterns, List<string> allFolders = null, string searchPattern = "*.*")
    {
        if (allFolders == null) allFolders = [];

        foreach (var folder in ExcludeFolders(folderPath, excludeSearchPatterns).ToList())
        {
            allFolders.Add(folder);
            GetFoldersRecursion(folder, excludeSearchPatterns, allFolders);
        }

        return allFolders;
    }

    private static IEnumerable<string> ExcludeFolders(string folderPath, List<string> excludeSearchPatterns, string searchPattern = "*.*")
    {
        var folders = Directory.EnumerateDirectories(folderPath, searchPattern, SearchOption.TopDirectoryOnly);
        foreach (var excludeSearchPattern in excludeSearchPatterns)
        {
            folders = folders.Where(f => !f.ToLowerInvariant().Contains(excludeSearchPattern)).ToList();
        }
        return folders;
    }

    private static IEnumerable<string> ExcludeFiles(string folderPath, List<string> excludeSearchPatterns, string searchPattern = "*.*")
    {
        var files = searchPattern.Split(Separator).SelectMany(s => Directory.EnumerateFiles(folderPath, s, SearchOption.TopDirectoryOnly)).ToList();
        foreach (var excludeSearchPattern in excludeSearchPatterns)
        {
            files = files.Where(f => !f.ToLowerInvariant().Contains(excludeSearchPattern)).ToList();
        }
        return files;
    }
}