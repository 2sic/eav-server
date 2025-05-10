using System.Linq;

namespace ToSic.Lib.Logging;

public partial class Log
{
    /// <summary>
    /// Link this logger to a parent
    /// and optionally rename
    /// </summary>
    /// <param name="newParent">parent log to attach to</param>
    /// <param name="name">optional new name</param>
    internal void LinkTo(ILog? newParent, string? name = default)
    {
        if (newParent == this)
            throw new("LOG ERROR - attaching a log to itself can't work");

        // only attach new parent if it didn't already have an old one
        // this is critical because we cannot guarantee that sometimes a LinkTo is called more than once on something
        if (newParent != null)
        {
            var oldParentTyped = Parent as Log;
            var newParentTyped = newParent as Log;

            // Only allow switching if the target doesn't have a parent
            // or if it was auto-linked but never used yet
            if (Parent == null || !Entries.Any())
            {
                Parent = newParent;
                Depth = newParentTyped?.Depth + 1 ?? 0;
                if (Depth > MaxParentDepth)
                    throw new($"🪵 LOGGER ERROR - Adding parent to logger exceeded max depth of {MaxParentDepth}");

                // If we have any entries that were added before, add them to the parent now
                if (Entries.Any() && newParentTyped != null)
                    Entries.ForEach(newParentTyped.AddEntry);
            }
            // show info if the new parent is different from the old one
            else if (oldParentTyped?.FullIdentifier != newParentTyped?.FullIdentifier)
                this.A("🪵 LOGGER INFO - logger with parent trying to attach. " +
                       $"Existing parent: {oldParentTyped?.FullIdentifier}. " +
                       $"New Parent (ignored): {newParentTyped?.FullIdentifier}");
        }

        if (name != null)
            this.Rename(name);
    }

    // 2025-03-26 2dm commented out, seems to be unused
    ///// <summary>
    ///// Would unlink this - but unclear if ever to be used
    ///// </summary>
    //public void Unlink() => Parent = null;

}