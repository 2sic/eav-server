namespace ToSic.Eav.Apps.State;

public interface IAppStateChanges
{
    event EventHandler AppStateChanged;
}