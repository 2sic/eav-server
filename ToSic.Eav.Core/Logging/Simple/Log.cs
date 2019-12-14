﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log: ILog
    {
        // unique ID of this logger, to not confuse it with other loggers
        private readonly string _id = Guid.NewGuid().ToString().Substring(0, 2);

        // ReSharper disable once StringLiteralTypo
        protected string Name = "unknwn";
        protected string Scope = "tdo";
        private const int MaxScopeLen = 3;
        private const int MaxNameLen = 6;
        public DateTime Created { get; } = DateTime.Now;

        public int WrapDepth;
        public  List<Entry> Entries { get; } = new List<Entry>();
        private ILog _parent;

        private string Identifier => $"{Scope}{Name}[{_id}]";

        public string FullIdentifier => _parent?.FullIdentifier + Identifier;

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

        public ILog AddChild(string name, string message=null) => new Log(name, this, message);

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
            _parent = parent ?? _parent;
            if (name != null)
                Rename(name);
        }


    }
}
