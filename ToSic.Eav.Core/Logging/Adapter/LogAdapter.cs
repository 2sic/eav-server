using System;
using System.Runtime.CompilerServices;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Logging
{
    public class LogAdapter : ILog, Lib.Logging.ILog // inherits both interfaces because of extension methods
    {
        private readonly Lib.Logging.ILog _log;

        /// <summary>
        /// For internal use only. Provide "Log" compatibility for LogAdapter
        /// </summary>
        [PrivateApi]
        public Log L => _log as Log;

        public LogAdapter(Lib.Logging.ILog log) => _log = log;

        /// <inheritdoc />
        public string Id => _log.Id;

        /// <inheritdoc />
        public string Identifier => _log.Identifier;

        /// <inheritdoc />
        public bool Preserve
        {
            get => _log.Preserve;
            set => _log.Preserve = value;
        }

        /// <inheritdoc />
        public int Depth
        {
            get => _log.Depth;
            set => _log.Depth = value;
        }

        /// <inheritdoc />
        public Action<string> Call(
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => (x) => _log.Fn<string>(parameters, message, useTimer, null, cPath, cName, cLine);

        /// <inheritdoc />
        public Action<string> Call(
            Func<string> parameters,
            Func<string> message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => (x) => _log.Fn<string>(parameters, message, useTimer, cPath, cName, cLine);

        /// <inheritdoc />
        public Func<string, T, T> Call<T>(
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => (x, t1) => _log.Fn<T>(parameters, message, useTimer, null, cPath, cName, cLine).Return(t1);

        /// <inheritdoc />
        public string Add(string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => _log.AddAndReuse(message, cPath, cName, cLine);

        /// <inheritdoc />
        public void Warn(string message) => _log.W(message);

        /// <inheritdoc />
        public void Add(Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => _log.A(messageMaker, cPath, cName, cLine);

        public void Exception(Exception ex) => _log.Ex(ex);

        #region Log properties

        [PrivateApi]
        public int WrapDepth
        {
            get => L.WrapDepth;
            set => L.WrapDepth = value;
        }

        #endregion
    }
}


