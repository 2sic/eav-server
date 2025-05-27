using System.Runtime.Caching;

namespace ToSic.Lib.Caching.Monitors;

internal class FolderChangeEventTarget
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