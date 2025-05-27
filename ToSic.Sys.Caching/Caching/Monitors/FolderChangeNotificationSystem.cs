using System.Collections;
using System.Runtime.Caching;
using System.Runtime.Caching.Hosting;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Caching.CachingMonitors;

//  based on https://github.com/microsoft/referencesource/blob/master/System.Runtime.Caching/System/Caching/FileChangeNotificationSystem.cs
[ShowApiWhenReleased(ShowApiMode.Never)]
public class FolderChangeNotificationSystem : IFileChangeNotificationSystem
{
    private readonly Hashtable _dirMonitors = Hashtable.Synchronized(new(StringComparer.OrdinalIgnoreCase));
    private readonly object _lock = new();

    public void StartMonitoring(string dirPath, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWriteTime, out long fileSize)
    {
        StartMonitoring(dirPath, true, onChangedCallback, out var typedState, out lastWriteTime, out fileSize);
        state = typedState;
    }

    internal void StartMonitoring(string dirPath, bool includeSubdirectories, OnChangedCallback onChangedCallback, out FolderChangeEventTarget state, out DateTimeOffset lastWriteTime, out long fileSize)
    {
        if (dirPath == null)
            throw new ArgumentNullException(nameof(dirPath));
        if (onChangedCallback == null)
            throw new ArgumentNullException(nameof(onChangedCallback));
            
        var directoryInfo = new DirectoryInfo(dirPath);
        var key = GetKey(dirPath, includeSubdirectories);
        if (_dirMonitors[key] is not DirectoryMonitor dirMon)
        {
            lock (_lock)
            {
                dirMon = _dirMonitors[key] as DirectoryMonitor ?? new DirectoryMonitor
                {
                    FileSystemWatcher = new(dirPath)
                    {
                        NotifyFilter = NotifyFilters.FileName
                                       | NotifyFilters.DirectoryName
                                       | NotifyFilters.Size
                                       | NotifyFilters.LastWrite,
                        IncludeSubdirectories = includeSubdirectories,
                        EnableRaisingEvents = true
                    }
                };
                _dirMonitors[key] = dirMon;
            }
        }

        var target = new FolderChangeEventTarget(/*directoryInfo.Name, */onChangedCallback);

        lock (dirMon)
        {
            dirMon.FileSystemWatcher.Changed += target.ChangedHandler;
            dirMon.FileSystemWatcher.Created += target.ChangedHandler;
            dirMon.FileSystemWatcher.Deleted += target.ChangedHandler;
            dirMon.FileSystemWatcher.Error += target.ErrorHandler;
            dirMon.FileSystemWatcher.Renamed += target.RenamedHandler;
        }

        state = target;
        lastWriteTime = directoryInfo.LastWriteTime;
        fileSize = (directoryInfo.Exists) ? /*GetDirectorySize(directoryInfo)*/ 0 : -1;
    }

    private static string GetKey(string dirPath, bool includeSubdirectories) 
        => (includeSubdirectories ? $"{dirPath}*" : dirPath).Backslash();

    // this is very slow in case of many subfolders and files, so we will not use it.
    //private static long GetDirectorySize(DirectoryInfo directoryInfo) => 
    //    directoryInfo.GetFiles("*.*",SearchOption.AllDirectories).Sum(f => f.Length);

    internal void StopMonitoring(string dirPath, bool includeSubdirectories, FolderChangeEventTarget target)
    {
        if (dirPath == null)
            throw new ArgumentNullException(nameof(dirPath));
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        var key = GetKey(dirPath, includeSubdirectories);
        if (_dirMonitors[key] is not DirectoryMonitor dirMon)
            return;

        lock (dirMon)
        {
            dirMon.FileSystemWatcher.Changed -= target.ChangedHandler;
            dirMon.FileSystemWatcher.Created -= target.ChangedHandler;
            dirMon.FileSystemWatcher.Deleted -= target.ChangedHandler;
            dirMon.FileSystemWatcher.Error -= target.ErrorHandler;
            dirMon.FileSystemWatcher.Renamed -= target.RenamedHandler;
        }
    }

    public void StopMonitoring(string dirPath, object state)
        => StopMonitoring(dirPath, true,
            state as FolderChangeEventTarget
            ?? throw new ArgumentException($"target is not {nameof(FolderChangeEventTarget)}", nameof(state)));

}