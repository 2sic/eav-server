using System;

namespace ToSic.Eav.Apps.Run
{
    public interface IBlockIdentifier: IAppIdentity
    {
        /// <summary>
        /// The block identifier
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// The preview-view identifies a view in an App
        /// It's only used when there is no content-block yet (so Guid is empty)
        /// Once an app has been fully added to the page, the PreviewView isn't used any more
        /// </summary>
        Guid PreviewView { get; }
    }
}
