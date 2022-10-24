using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Trivial interface just to ensure that we have debug on/off consistent
    /// </summary>
    [PrivateApi("WIP")]
    public interface ICanDebug
    {
        bool Debug { get; set; }
    }
}
