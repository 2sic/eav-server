using System;
using System.Collections;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Caching.Hosting;

namespace ToSic.Eav.Caching.CachingMonitors;

//  based on https://github.com/microsoft/referencesource/blob/master/System.Runtime.Caching/System/Caching/FileChangeNotificationSystem.cs
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FolderChangeNotificationSystem : IFileChangeNotificationSystem
{
    private readonly Hashtable _dirMonitors = Hashtable.Synchronized(new(StringComparer.OrdinalIgnoreCase));
    private readonly object _lock = new();

    public class DirectoryMonitor
    {
        public FileSystemWatcher FileSystemWatcher;
    }

    public class FolderChangeEventTarget
    {
        //private readonly string _folderName;
        private readonly OnChangedCallback _onChangedCallback;
        public FileSystemEventHandler ChangedHandler { get; }
        public ErrorEventHandler ErrorHandler { get; }
        public RenamedEventHandler RenamedHandler { get; }
        public FolderChangeEventTarget(/*string folderName, */OnChangedCallback onChangedCallback)
        {
            //_folderName = folderName;
            _onChangedCallback = onChangedCallback;
            ChangedHandler = new(this.OnChanged);
            ErrorHandler = new(this.OnError);
            RenamedHandler = new(this.OnRenamed);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            //if (EqualsIgnoreCase(_folderName, e.Name))
            _onChangedCallback(null);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _onChangedCallback(null);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            //if (EqualsIgnoreCase(_folderName, e.Name) || EqualsIgnoreCase(_folderName, e.OldName))
            _onChangedCallback(null);
        }
            
        //private static bool EqualsIgnoreCase(string s1, string s2)
        //{
        //    if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
        //        return true;
        //    if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        //        return false;
        //    if (s2.Length != s1.Length)
        //        return false;
        //    return 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
        //}
    }
    public void StartMonitoring(string dirPath, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWriteTime, out long fileSize) =>
        StartMonitoring(dirPath, true, onChangedCallback, out state, out lastWriteTime, out fileSize);

    public void StartMonitoring(string dirPath, bool includeSubdirectories, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWriteTime, out long fileSize)
    {
        if (dirPath == null) throw new ArgumentNullException(nameof(dirPath));
        if (onChangedCallback == null) throw new ArgumentNullException(nameof(onChangedCallback));
            
        var directoryInfo = new DirectoryInfo(dirPath);
        if (_dirMonitors[dirPath] is not DirectoryMonitor dirMon)
        {
            lock (_lock)
            {
                dirMon = _dirMonitors[dirPath] as DirectoryMonitor ?? new DirectoryMonitor
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
                _dirMonitors[dirPath] = dirMon;
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

    // this is very slow in case of many subfolders and files, so we will not use it.
    //private static long GetDirectorySize(DirectoryInfo directoryInfo) => 
    //    directoryInfo.GetFiles("*.*",SearchOption.AllDirectories).Sum(f => f.Length);

    public void StopMonitoring(string dirPath, object state)
    {
        if (dirPath == null) throw new ArgumentNullException(nameof(dirPath));
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (state is not FolderChangeEventTarget target) throw new ArgumentException("target is null");
        if (_dirMonitors[dirPath] is not DirectoryMonitor dirMon) return;
        lock (dirMon)
        {
            dirMon.FileSystemWatcher.Changed -= target.ChangedHandler;
            dirMon.FileSystemWatcher.Created -= target.ChangedHandler;
            dirMon.FileSystemWatcher.Deleted -= target.ChangedHandler;
            dirMon.FileSystemWatcher.Error -= target.ErrorHandler;
            dirMon.FileSystemWatcher.Renamed -= target.RenamedHandler;
        }
    }
}