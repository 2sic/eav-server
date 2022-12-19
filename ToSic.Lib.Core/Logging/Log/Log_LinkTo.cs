using System;

namespace ToSic.Lib.Logging
{
    public partial class Log
    {
        /// <summary>
        /// Link this logger to a parent
        /// and optionally rename
        /// </summary>
        /// <param name="newParent">parent log to attach to</param>
        /// <param name="name">optional new name</param>
        internal void LinkTo(ILog newParent, string name = null)
        {
            if (newParent == this) throw new Exception("LOG ERROR - attaching a log to itself can't work");
            // only attach new parent if it didn't already have an old one
            // this is critical because we cannot guarantee that sometimes a LinkTo is called more than once on something
            if (newParent != null)
            {
                if (Parent == null)
                {
                    Parent = newParent;
                    Depth = (newParent as Log)?.Depth + 1 ?? 0;
                    if (Depth > MaxParentDepth)
                        throw new Exception(
                            $"LOG ERROR - Adding parent to logger but exceeded max depth of {MaxParentDepth}");
                }
                // show an error, if it the new parent is different from the old one
                else if ((Parent as Log)?.FullIdentifier != (newParent as Log)?.FullIdentifier)
                    this.A("LOG INFO - this logger already has a parent, but trying to attach to new parent. " +
                           $"Existing parent: {(Parent as Log)?.FullIdentifier}. " +
                           $"New Parent (ignored): {(newParent as Log)?.FullIdentifier}");
            }

            if (name != null)
                this.Rename(name);
        }

        /// <summary>
        /// Would unlink this - but unclear if ever to be used
        /// </summary>
        public void Unlink() => Parent = null;

    }
}
