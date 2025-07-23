namespace BMAP.Core.Result;

/// <summary>
///     Represents the different types/categories of errors that can occur in an application.
///     This enumeration helps categorize errors for better error handling and user experience.
/// </summary>
public enum ErrorType
{
    /// <summary>
    ///     Represents no error (success case).
    /// </summary>
    None = 0,

    /// <summary>
    ///     General error type for uncategorized errors.
    /// </summary>
    General = 1,

    /// <summary>
    ///     Validation error - occurs when input data doesn't meet validation criteria.
    /// </summary>
    Validation = 2,

    /// <summary>
    ///     Not found error - occurs when a requested resource cannot be found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    ///     Conflict error - occurs when there's a conflict with the current state of the resource.
    /// </summary>
    Conflict = 4,

    /// <summary>
    ///     Unauthorized error - occurs when authentication is required but not provided.
    /// </summary>
    Unauthorized = 5,

    /// <summary>
    ///     Forbidden error - occurs when the user doesn't have permission to access the resource.
    /// </summary>
    Forbidden = 6,

    /// <summary>
    ///     Internal error - occurs when there's an internal system error.
    /// </summary>
    Internal = 7,

    /// <summary>
    ///     External error - occurs when an external system or dependency fails.
    /// </summary>
    External = 8,

    /// <summary>
    ///     Custom error type for application-specific errors.
    /// </summary>
    Custom = 9
}