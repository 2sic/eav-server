using System;

namespace ToSic.Eav.Apps.Run
{
    public class BlockIdentifier: IBlockIdentifier
    {
        public BlockIdentifier(int zoneId, int appId, Guid guid, Guid viewOverride)
        {
            ZoneId = zoneId;
            AppId = appId;
            Guid = guid;
            PreviewView = viewOverride;
        }

        public int ZoneId { get; }
        public int AppId { get; }
        public Guid Guid { get; }
        public Guid PreviewView { get; }
    }
}
