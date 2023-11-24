using System;

namespace ToSic.Eav.Apps.Run;

/// <inheritdoc />
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class BlockIdentifier: IBlockIdentifier
{
    public BlockIdentifier(int zoneId, int appId, string appNameId, Guid guid, Guid viewOverride)
    {
        ZoneId = zoneId;
        AppId = appId;
        AppNameId = appNameId;
        Guid = guid;
        PreviewView = viewOverride;
    }

    /// <inheritdoc />
    public int ZoneId { get; }
    /// <inheritdoc />
    public int AppId { get; }

    public string AppNameId { get; }

    /// <inheritdoc />
    public Guid Guid { get; }
    /// <inheritdoc />
    public Guid PreviewView { get; }
}