﻿using System;
using System.Runtime.CompilerServices;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Logging
{
    // inherit from wrapper class
    public class LogAdapter : Wrapper<Lib.Logging.ILog>, ILog
    {
        public LogAdapter(Lib.Logging.ILog log) : base(log ?? new Log(LogConstants.NameUnknown))
        { }

        /// <inheritdoc />
        public string Id => _contents.Id;

        /// <inheritdoc />
        public string Identifier => _contents.Identifier;

        /// <inheritdoc />
        public bool Preserve
        {
            get => _contents.Preserve;
            set => _contents.Preserve = value;
        }

        /// <inheritdoc />
        public int Depth
        {
            get => _contents.Depth;
            set => _contents.Depth = value;
        }

        /// <inheritdoc />
        public Action<string> Call(
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => (x) => _contents.Fn<string>(parameters, message, useTimer, null, cPath, cName, cLine);

        /// <inheritdoc />
        public Action<string> Call(
            Func<string> parameters,
            Func<string> message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => (x) => _contents.Fn<string>(parameters, message, useTimer, cPath, cName, cLine);

        /// <inheritdoc />
        public Func<string, T, T> Call<T>(
            string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => (x, t1) => _contents.Fn<T>(parameters, message, useTimer, null, cPath, cName, cLine).Return(t1);

        /// <inheritdoc />
        public string Add(string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => _contents.AddAndReuse(message, cPath, cName, cLine);

        /// <inheritdoc />
        public void Warn(string message) => _contents.W(message);

        /// <inheritdoc />
        public void Add(Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => _contents.A(messageMaker, cPath, cName, cLine);

        public void Exception(Exception ex) => _contents.Ex(ex);
    }
}


