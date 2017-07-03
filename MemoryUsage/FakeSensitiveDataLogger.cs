using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MemoryUsage
{
    public class FakeSensitiveDataLogger<T> : ISensitiveDataLogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        public bool LogSensitiveData { get; }

        public IDisposable BeginScope(object state)
        {
            throw new NotImplementedException();
        }
    }
}