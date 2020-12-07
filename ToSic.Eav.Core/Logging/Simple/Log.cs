using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log: ILog
    {
        // unique ID of this logger, to not confuse it with other loggers
        public string Id { get; } = Guid.NewGuid().ToString().Substring(0, 2);

        // ReSharper disable once StringLiteralTypo
        protected string Name = "unknwn";
        protected string Scope = "tdo";
        private const int MaxScopeLen = 3;
        private const int MaxNameLen = 6;
        public DateTime Created { get; } = DateTime.Now;

        public int WrapDepth;
        public  List<Entry> Entries { get; } = new List<Entry>();
        private ILog _parent;
        public int Depth { get; set; } = 0;
        private const int MaxParentDepth = 100;

        public string Identifier => $"{Scope}{Name}[{Id}]";

        public string FullIdentifier => _parent?.FullIdentifier + Identifier;

        public bool Preserve
        {
            get => _preserve;
            set
            {
                _preserve = value;
                // pass it on to the parent, so that the chain knows if it should be preserved
                if (_parent != null) _parent.Preserve = true;
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
            Rename(name);
            LinkTo(parent);
            if (initialMessage == null) return;
            AddInternal(initialMessage, code);
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
        : this(name, parent, new CodeRef(cPath, cName, cLine), initialMessage)
        {}


        /// <summary>
        /// Rename this logger - usually used when a base-class has a logger, 
        /// but the inherited class needs a different name
        /// </summary>
        /// <remarks>
        /// limits the length to 6 chars to make the output readable
        /// </remarks>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            if (name == null) return;
            try
            {
                var dot = name.IndexOf(".", StringComparison.Ordinal);
                Scope = dot > 0 ? name.Substring(0, Math.Min(dot, MaxScopeLen)) + "." : "";
                var rest = dot > 0 ? name.Substring(dot + 1) : name;
                Name = rest.Substring(0, Math.Min(rest.Length, MaxNameLen));
                Name = Name.Substring(0, Math.Min(Name.Length, MaxNameLen));
            }
            catch { /* ignore */ }
        }


        /// <summary>
        /// Link this logger to a parent
        /// and optionally rename
        /// </summary>
        /// <param name="parent">parent log to attach to</param>
        /// <param name="name">optional new name</param>
        public void LinkTo(ILog parent, string name = null)
        {
            if(parent == this) throw new Exception("LOGGER ERROR - attaching same item as parent can't work");
            // only attach new parent if it didn't already have an old one
            // this is critical because we cannot guarantee that sometimes a LinkTo is called more than once on something
            if (parent != null ) 
            {
                if(_parent == null)
                {
                    _parent = parent;
                    Depth = parent.Depth + 1;
                    if(Depth > MaxParentDepth)
                        throw new Exception($"LOGGER ERROR - Adding parent to logger but exceeded max depth of {MaxParentDepth}");
                }
                // show an error, if it the new parent is different from the old one
                else if (_parent.FullIdentifier != parent.FullIdentifier)
                    Add("LOGGER WARNING - this logger already has a parent, but trying to attach to new parent. " +
                        $"Existing parent: {_parent.FullIdentifier}. " +
                        $"New Parent (ignored): {parent.FullIdentifier}");
            }
            if (name != null)
                Rename(name);
        }


    }
}
