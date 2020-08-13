using System;

namespace ToSic.Eav.Apps.Run
{
    /// <inheritdoc />
    public class BlockIdentifier: IBlockIdentifier
    {
        public BlockIdentifier(int zoneId, int appId, Guid guid, Guid viewOverride)
        {
            ZoneId = zoneId;
            AppId = appId;
            Guid = guid;
            PreviewView = viewOverride;
        }

        /// <inheritdoc />
        public int ZoneId { get; }
        /// <inheritdoc />
        public int AppId { get; }
        /// <inheritdoc />
        public Guid Guid { get; }
        /// <inheritdoc />
        public Guid PreviewView { get; }
    }
}
