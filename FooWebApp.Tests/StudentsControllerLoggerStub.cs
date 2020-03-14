﻿using FooWebApp.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace FooWebApp.Tests
{
    public class StudentsControllerLoggerStub : ILogger<StudentsController>
    {
        public List<LogEntry> LogEntries = new List<LogEntry>();
        
        public IDisposable BeginScope<TState>(TState state)
        {
            return new MemoryStream();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogEntries.Add(new LogEntry
            {
                Level = logLevel,
                EventId = eventId,
                Exception = exception
            });
        }

        public class LogEntry
        {
            public LogLevel Level { get; set; }
            public EventId EventId { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
