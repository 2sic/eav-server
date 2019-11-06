using System;

// ReSharper disable once CheckNamespace
namespace ToSic.Sxc.Adam
{
    /// <summary>
    /// Old compatibility class - was previously in public use for example in Mobius Forms
    /// </summary>
    public interface IFile
    {
        #region Metadata
        bool HasMetadata { get; }

        dynamic Metadata { get; }
        #endregion

        string Url { get; }

        string Type { get; }

        [Obsolete("use Id instead")]
        int FileId { get; }

        [Obsolete("use FullName instead")]
        string FileName { get; }

    };
}

