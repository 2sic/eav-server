using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{
    public partial class Log: ILog, ILogInternal
    {
        /// <summary>
        /// Max logging depth, we should never attach loggers if we are past this level
        /// </summary>
        private const int MaxParentDepth = 100;

        #region Constructors

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

        #endregion

        #region Name and Identifiers

        /// <summary>
        /// Unique ID of this logger, to not confuse it with other loggers.
        /// Is usually shown in the logs right after the name as [xy]
        /// </summary>
        internal string Id { get; } = Guid.NewGuid().ToString().Substring(0, 2);

        /// <summary>
        /// The topic this log belongs to, like Eav, Sxc, etc.
        /// Max length is 3 chars.
        /// </summary>
        internal string Scope = LogConstants.ScopeUnknown;

        /// <summary>
        /// The current logs name.
        /// Max length is 6 chars.
        /// </summary>
        internal string Name = LogConstants.NameUnknown;

        /// <summary>
        /// A standardized identifier of this log for showing in protocols
        /// </summary>
        public string NameId => $"{Scope}{Name}[{Id}]";


        public string FullIdentifier => (Parent as Log)?.FullIdentifier + NameId;

        #endregion

        #region Self Reference (WIP)

        /// <inheritdoc />
        public ILog _RealLog => this;

        #endregion

        #region Properties

        public DateTime Created { get; } = DateTime.Now;

        /// <summary>
        /// How many inner wraps are currently open (when creating child entries using .Fn(...)
        /// </summary>
        internal int WrapDepth;

        /// <summary>
        /// Entries of this log and all children
        /// </summary>
        public  List<Entry> Entries { get; } = new List<Entry>();

        /// <summary>
        /// Parent to which it's linked
        /// </summary>
        public ILog Parent { get; private set; }

        /// <summary>
        /// How deep this logger is in the whole tree
        /// </summary>
        internal int Depth { get; set; }


        /// <inheritdoc />
        public bool Preserve
        {
            get => _preserve;
            set
            {
                _preserve = value;
                // pass it on to the parent if suddenly turned on, so that the chain knows if it should be preserved
                if (value && Parent != null) Parent.Preserve = true;
            }
        }
        private bool _preserve = true;

        #endregion
    }
}
