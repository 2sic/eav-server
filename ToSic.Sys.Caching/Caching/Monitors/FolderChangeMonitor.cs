using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Caching;
using System.Text;

namespace ToSic.Eav.Caching.CachingMonitors;

// based on https://raw.githubusercontent.com/microsoft/referencesource/master/System.Runtime.Caching/System/Caching/HostFileChangeMonitor.cs
[ShowApiWhenReleased(ShowApiMode.Never)]
public class FolderChangeMonitor : FileChangeMonitor
{
    private const int MaxCharCountOfLongConvertedToHexadecimalString = 16;
    private static FolderChangeNotificationSystem? _folderChangeNotificationSystemStatic;
    private Dictionary<string, FolderChangeEventTarget>? _fcnState;
    private FolderChangeEventTarget? _folderChangeState;

    public override ReadOnlyCollection<string> FilePaths => _folderPaths
        .Select(folderInfo => folderInfo.Key)
        .ToList()
        .AsReadOnly();
    private readonly IDictionary<string, bool> _folderPaths;

    public override string UniqueId { get; }

    public override DateTimeOffset LastModified => _lastModified;
    private DateTimeOffset _lastModified;

    //public FolderChangeMonitor(IList<string>? folderPaths)
    //    : this(folderPaths?.ToDictionary(p => p, p => true) ?? new Dictionary<string, bool>())
    //{ }

    public FolderChangeMonitor(IDictionary<string, bool> folderPaths)
    {
        if (folderPaths == null || folderPaths.Count == 0)
            throw new ArgumentException("Empty collection: folderPaths");

        _folderPaths = new ReadOnlyDictionary<string, bool>(folderPaths);

        // Init the FolderChangeNotificationSystem
        if (_folderChangeNotificationSystemStatic == null)
            Interlocked.CompareExchange(ref _folderChangeNotificationSystemStatic, new(), null);
        UniqueId = InitDisposableMembers();
    }

    private string InitDisposableMembers()
    {
        var dispose = true;
        string uniqueId;
        try
        {
            if (_folderPaths.Count == 1)
            {
                var path = _folderPaths.First();
                _folderChangeNotificationSystemStatic!
                    .StartMonitoring(path.Key, path.Value, OnChanged, out _folderChangeState, out var lastWrite, out var fileSize);
                uniqueId = path + lastWrite.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture) + fileSize.ToString("X", CultureInfo.InvariantCulture);
                _lastModified = lastWrite;
            }
            else
            {
                var capacity = _folderPaths.Sum(path => path.Key.Length + (2 * MaxCharCountOfLongConvertedToHexadecimalString));
                _fcnState = new(_folderPaths.Count);
                var sb = new StringBuilder(capacity);
                foreach (var path in _folderPaths)
                {
                    if (_fcnState.ContainsKey(path.Key))
                        continue;
                    _folderChangeNotificationSystemStatic!
                        .StartMonitoring(path.Key, path.Value, OnChanged, out var state, out var lastWrite, out var fileSize);
                    _fcnState[path.Key] = state;
                    sb.Append(path.Key);
                    sb.Append(lastWrite.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture));
                    sb.Append(fileSize.ToString("X", CultureInfo.InvariantCulture));
                    if (lastWrite > _lastModified)
                        _lastModified = lastWrite;
                }
                uniqueId = sb.ToString();
            }
            dispose = false;
        }
        finally
        {
            InitializationComplete();
            if (dispose)
                Dispose();
        }
        return uniqueId;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing || _folderChangeNotificationSystemStatic == null || _folderPaths == null || _fcnState == null)
            return;
            
        if (_folderPaths.Count > 1)
        {
            foreach (var path in _folderPaths)
            {
                if (string.IsNullOrEmpty(path.Key))
                    continue;
                    
                var state = _fcnState[path.Key];
                if (state != null)
                    _folderChangeNotificationSystemStatic.StopMonitoring(path.Key, path.Value, state);
            }
        }
        else
        {
            var path = _folderPaths.First();
            if (path.Key != null && _folderChangeState != null)
                _folderChangeNotificationSystemStatic.StopMonitoring(path.Key, path.Value, _folderChangeState);
        }
    }
}