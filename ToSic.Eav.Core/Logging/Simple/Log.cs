using System;
using System.Collections.Generic;
using System.Text;

namespace ToSic.Eav.Logging.Simple
{
    public class Log
    {
        // unique ID of this logger, to not confuse it with other loggers
        private readonly string _id = Guid.NewGuid().ToString().Substring(0, 2);

        private string _name = "unknwn";
        private string _scope = "tdo";
        private const int MaxScopeLen = 3;
        private const int MaxNameLen = 6;
        public  List<Entry> Entries { get; } = new List<Entry>();
        private Log _parent;

        private string Identifier => $"{_scope}{_name}({_id})";

        public string FullIdentifier => _parent?.FullIdentifier + Identifier;

        /// <summary>
        /// Create a logger and optionally attach it to a parent logger
        /// </summary>
        /// <param name="name">name this logger should use</param>
        /// <param name="parent">optional parrent logger to attach to</param>
        /// <param name="initialMessage">optional initial message to log</param>
        public Log(string name, Log parent = null, string initialMessage = null)
        {
            Rename(name);
            LinkTo(parent);
            if(initialMessage != null)
                Add(initialMessage);
        }

        public Log AddChild(string name, string message) => new Log(name, this, message);

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
                _scope = dot > 0 ? name.Substring(0, Math.Min(dot, MaxScopeLen)) + "." : "";
                var rest = dot > 0 ? name.Substring(dot + 1) : name;
                _name = rest.Substring(0, Math.Min(rest.Length, MaxNameLen));
                _name = _name.Substring(0, Math.Min(_name.Length, MaxNameLen));
            }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Add a message
        /// </summary>
        /// <param name="message"></param>
        public string Add(string message)
        {
            Add(new Entry(this, message));
            return message;
        }
        public string Warn(string message)
        {
            Add(new Entry(this, "WARNING: " + message));
            return message;
        }


        /// <summary>
        /// add an existing entry of another logger
        /// </summary>
        /// <param name="entry"></param>
        private void Add(Entry entry)
        {
            Entries.Add(entry);
            _parent?.Add(entry);
        }

        /// <summary>
        /// Add a message by calling a function. This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        public void Add(Func<string> messageMaker)
        {
            try
            {
                Add(messageMaker.Invoke());
            }
            catch (Exception ex)
            {
                Add("Log: failed to add message from code, error was: " + ex.Message);
            }
        }

        /// <summary>
        /// Link this logger to a parent
        /// and optionally rename
        /// </summary>
        /// <param name="parent">parent log to attach to</param>
        /// <param name="name">optional new name</param>
        public void LinkTo(Log parent, string name = null)
        {
            _parent = parent ?? _parent;
            if (name != null)
                Rename(name);
        }

        public string Dump(string separator = " - ", string start = "", string end = "")
        {
            var lg = new StringBuilder(start);
            Entries.ForEach(e => lg.AppendLine(e.Source + separator + e.Message));
            lg.Append(end);
            return lg.ToString();
        }
    }
}
