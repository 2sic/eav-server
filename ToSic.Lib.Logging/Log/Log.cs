using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{
    public class Log: ILog
    {
        // unique ID of this logger, to not confuse it with other loggers
        public string Id { get; } = Guid.NewGuid().ToString().Substring(0, 2);

        internal string Name = LogConstants.NameUnknown;
        internal string Scope = LogConstants.ScopeUnknown;
        public DateTime Created { get; } = DateTime.Now;

        public int WrapDepth;
        public  List<Entry> Entries { get; } = new List<Entry>();
        public ILog Parent { get; private set; }
        public int Depth { get; set; }
        private const int MaxParentDepth = 100;

        public string Identifier => $"{Scope}{Name}[{Id}]";

        public string FullIdentifier => (Parent as Log)?.FullIdentifier + Identifier;

        public bool Preserve
        {
            get => _preserve;
            set
            {
                _preserve = value;
                // pass it on to the parent, so that the chain knows if it should be preserved
                if (Parent != null) Parent.Preserve = true;
            }
        }

        private bool _preserve = true;

        /// <summary>
        /// Create a logger and optionally attach it to a parent logger
        /// </summary>
        /// <param name="name">name this logger should use</param>
        /// <param name="parent">optional parent logger to attach to</param>
        /// <param name="code">The code reference - must be generated before</param>
        /// <param name="initialMessage">optional initial message to log</param>
        public Log(string name, ILog parent, CodeRef code, string initialMessage = null)
        {
            this.Rename(name);
            LinkTo(parent);
            if (initialMessage == null) return;
            this.AddInternal(initialMessage, code);
        }

        /// <summary>
        /// Create a logger and optionally attach it to a parent logger
        /// </summary>
        /// <param name="name">name this logger should use</param>
        /// <param name="parent">optional parent logger to attach to</param>
        /// <param name="initialMessage">optional initial message to log</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        public Log(string name, ILog parent = null, string initialMessage = null,
                [CallerFilePath] string cPath = null,
                [CallerMemberName] string cName = null,
                [CallerLineNumber] int cLine = 0)
        : this(name, parent, new CodeRef(cPath, cName, cLine), initialMessage) { }

        /// <summary>
        /// Link this logger to a parent
        /// and optionally rename
        /// </summary>
        /// <param name="newParent">parent log to attach to</param>
        /// <param name="name">optional new name</param>
        internal void LinkTo(ILog newParent, string name = null)
        {
            if(newParent == this) throw new Exception("LOGGER ERROR - attaching same item as parent can't work");
            // only attach new parent if it didn't already have an old one
            // this is critical because we cannot guarantee that sometimes a LinkTo is called more than once on something
            if (newParent != null) 
            {
                if(Parent == null)
                {
                    Parent = newParent;
                    Depth = newParent.Depth + 1;
                    if(Depth > MaxParentDepth)
                        throw new Exception($"LOGGER ERROR - Adding parent to logger but exceeded max depth of {MaxParentDepth}");
                }
                // show an error, if it the new parent is different from the old one
                else if ((Parent as Log)?.FullIdentifier != (newParent as Log)?.FullIdentifier)
                    this.A("LOGGER INFO - this logger already has a parent, but trying to attach to new parent. " +
                        $"Existing parent: {(Parent as Log)?.FullIdentifier}. " +
                        $"New Parent (ignored): {(newParent as Log)?.FullIdentifier}");
            }
            if (name != null)
                this.Rename(name);
        }

        /// <inheritdoc />
        public void Unlink() => Parent = null;
    }
}
