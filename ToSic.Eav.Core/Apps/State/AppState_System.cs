namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        public AppStateSettings SettingsInApp => _settingsInApp ?? (_settingsInApp = new AppStateSettings(this, _appStates));
        private AppStateSettings _settingsInApp;
    }
}
