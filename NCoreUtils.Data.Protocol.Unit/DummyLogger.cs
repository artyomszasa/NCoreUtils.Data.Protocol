using System;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Data.Protocol.Unit;

public class DummyLogger : ILogger
{
    public sealed class DummyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
        => new DummyDisposable();

    public bool IsEnabled(LogLevel logLevel)
        => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    { }
}

public class DummyLogger<T> : DummyLogger, ILogger<T> { }