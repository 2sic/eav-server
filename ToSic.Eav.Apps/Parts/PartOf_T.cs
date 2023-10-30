﻿using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Base class for any kind of read/runtime operations
    /// </summary>
    public abstract class PartOf<TParent> : ServiceBase
    {
        // ReSharper disable once InconsistentNaming
        protected internal TParent Parent { get; internal set; }

        protected PartOf(string logName): base(logName) { }
    }

    public static class PartOfExtensions
    {
        public static TInit ConnectTo<TParent, TInit>(this TInit target, TParent parent) where TInit : PartOf<TParent>
        {
            target.Parent = parent;
            return target;
        }
    }
}
