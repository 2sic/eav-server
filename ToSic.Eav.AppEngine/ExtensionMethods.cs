namespace ToSic.Eav.Apps
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Text based identity for debugging only
        /// </summary>
        public static string LogState(this IInAppAndZone withAppIdentity) 
            => withAppIdentity.ZoneId + ":" + withAppIdentity.AppId;
    }
}
