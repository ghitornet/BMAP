using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Helper class for creating mock loggers for testing purposes.
/// </summary>
public static class MockLoggerHelper
{
    /// <summary>
    /// Creates a null logger that doesn't perform any logging operations.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <returns>A null logger instance.</returns>
    public static ILogger<T> CreateNullLogger<T>()
    {
        return NullLogger<T>.Instance;
    }

    /// <summary>
    /// Creates a logger that writes to a StringBuilder for testing purposes.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <param name="output">The StringBuilder to write logs to.</param>
    /// <returns>A logger instance that writes to the provided StringBuilder.</returns>
    public static ILogger<T> CreateLogger<T>(StringBuilder output)
    {
        return new TestLogger<T>(output);
    }
}

/// <summary>
/// Test logger implementation that writes to a StringBuilder.
/// </summary>
/// <typeparam name="T">The category type for the logger.</typeparam>
public class TestLogger<T> : ILogger<T>
{
    private readonly StringBuilder _output;

    public TestLogger(StringBuilder output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _output.AppendLine($"[{logLevel}] {message}");
        if (exception != null)
        {
            _output.AppendLine($"Exception: {exception}");
        }
    }
}