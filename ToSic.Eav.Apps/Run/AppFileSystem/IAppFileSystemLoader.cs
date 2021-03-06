﻿using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Run
{
    public interface IAppFileSystemLoader
    {
        /// <summary>
        /// Real constructor, after DI
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="path"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        IAppFileSystemLoader Init(int appId, string path, ILog log);

        string Path { get; set; }

        /// <summary>
        /// Load all the input types for this app from the folder
        /// </summary>
        /// <returns></returns>
        List<InputTypeInfo> InputTypes();
    }
}
