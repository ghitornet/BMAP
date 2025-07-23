using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
}