namespace BMAP.Core.Data.EntityFramework.Abstractions;

/// <summary>
/// Provides information about the current user for audit and security purposes.
/// This interface should be implemented to provide user context in different environments
/// (Web, Console, Service, etc.).
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the username or display name of the current user.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the email of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the roles associated with the current user.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Gets additional claims or properties associated with the current user.
    /// </summary>
    IDictionary<string, string> Properties { get; }

    /// <summary>
    /// Indicates whether a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the tenant identifier for multi-tenant scenarios.
    /// </summary>
    string? TenantId { get; }
}

/// <summary>
/// Default implementation of IUserContext for scenarios where no user is available.
/// </summary>
public class SystemUserContext : IUserContext
{
    public string? UserId => "SYSTEM";
    public string? UserName => "System";
    public string? Email => null;
    public IEnumerable<string> Roles => new[] { "System" };
    public IDictionary<string, string> Properties => new Dictionary<string, string>();
    public bool IsAuthenticated => true;
    public string? TenantId => null;
}

/// <summary>
/// User context implementation for ASP.NET Core web applications.
/// Note: This requires Microsoft.AspNetCore.Http package to be installed for full functionality.
/// </summary>
public class WebUserContext : IUserContext
{
    private readonly object? _httpContextAccessor;

    public WebUserContext(object? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            try
            {
                // Use reflection to access HttpContext without direct dependency
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                if (user == null) return null;

                return FindClaim(user, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    ?? FindClaim(user, "sub")
                    ?? FindClaim(user, "user_id");
            }
            catch
            {
                return null;
            }
        }
    }

    public string? UserName
    {
        get
        {
            try
            {
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                if (user == null) return null;

                return FindClaim(user, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    ?? FindClaim(user, "preferred_username")
                    ?? FindClaim(user, "username");
            }
            catch
            {
                return null;
            }
        }
    }

    public string? Email
    {
        get
        {
            try
            {
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                if (user == null) return null;

                return FindClaim(user, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                    ?? FindClaim(user, "email");
            }
            catch
            {
                return null;
            }
        }
    }

    public IEnumerable<string> Roles
    {
        get
        {
            try
            {
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                if (user == null) return Enumerable.Empty<string>();

                return FindAllClaims(user, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Concat(FindAllClaims(user, "role"))
                    .ToList();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
    }

    public IDictionary<string, string> Properties
    {
        get
        {
            try
            {
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                if (user == null) return new Dictionary<string, string>();

                var claims = GetAllClaims(user);
                return claims
                    .Where(c => !IsStandardClaim(c.Key))
                    .ToDictionary(c => c.Key, c => c.Value);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            try
            {
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                var identity = GetIdentity(user);
                return GetIsAuthenticated(identity);
            }
            catch
            {
                return false;
            }
        }
    }

    public string? TenantId
    {
        get
        {
            try
            {
                var httpContext = GetHttpContext();
                var user = GetUser(httpContext);
                if (user == null) return null;

                return FindClaim(user, "tenant_id")
                    ?? FindClaim(user, "tid");
            }
            catch
            {
                return null;
            }
        }
    }

    #region Private Reflection-Based Methods

    private object? GetHttpContext()
    {
        if (_httpContextAccessor == null) return null;

        var httpContextProperty = _httpContextAccessor.GetType().GetProperty("HttpContext");
        return httpContextProperty?.GetValue(_httpContextAccessor);
    }

    private object? GetUser(object? httpContext)
    {
        if (httpContext == null) return null;

        var userProperty = httpContext.GetType().GetProperty("User");
        return userProperty?.GetValue(httpContext);
    }

    private object? GetIdentity(object? user)
    {
        if (user == null) return null;

        var identityProperty = user.GetType().GetProperty("Identity");
        return identityProperty?.GetValue(user);
    }

    private bool GetIsAuthenticated(object? identity)
    {
        if (identity == null) return false;

        var isAuthenticatedProperty = identity.GetType().GetProperty("IsAuthenticated");
        return (bool)(isAuthenticatedProperty?.GetValue(identity) ?? false);
    }

    private string? FindClaim(object? user, string claimType)
    {
        if (user == null) return null;

        try
        {
            var findFirstMethod = user.GetType().GetMethod("FindFirst", new[] { typeof(string) });
            var claim = findFirstMethod?.Invoke(user, new object[] { claimType });
            
            if (claim == null) return null;

            var valueProperty = claim.GetType().GetProperty("Value");
            return valueProperty?.GetValue(claim) as string;
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<string> FindAllClaims(object? user, string claimType)
    {
        if (user == null) return Enumerable.Empty<string>();

        try
        {
            var findAllMethod = user.GetType().GetMethod("FindAll", new[] { typeof(string) });
            var claims = findAllMethod?.Invoke(user, new object[] { claimType });
            
            if (claims is not System.Collections.IEnumerable enumerable) return Enumerable.Empty<string>();

            var result = new List<string>();
            foreach (var claim in enumerable)
            {
                var valueProperty = claim.GetType().GetProperty("Value");
                var value = valueProperty?.GetValue(claim) as string;
                if (value != null)
                {
                    result.Add(value);
                }
            }
            return result;
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    private IEnumerable<KeyValuePair<string, string>> GetAllClaims(object? user)
    {
        if (user == null) return Enumerable.Empty<KeyValuePair<string, string>>();

        try
        {
            var claimsProperty = user.GetType().GetProperty("Claims");
            var claims = claimsProperty?.GetValue(user);
            
            if (claims is not System.Collections.IEnumerable enumerable) return Enumerable.Empty<KeyValuePair<string, string>>();

            var result = new List<KeyValuePair<string, string>>();
            foreach (var claim in enumerable)
            {
                var typeProperty = claim.GetType().GetProperty("Type");
                var valueProperty = claim.GetType().GetProperty("Value");
                
                var type = typeProperty?.GetValue(claim) as string;
                var value = valueProperty?.GetValue(claim) as string;
                
                if (type != null && value != null)
                {
                    result.Add(new KeyValuePair<string, string>(type, value));
                }
            }
            return result;
        }
        catch
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }
    }

    private static bool IsStandardClaim(string claimType)
    {
        return claimType switch
        {
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => true,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => true,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => true,
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => true,
            "sub" => true,
            "preferred_username" => true,
            "username" => true,
            "email" => true,
            "role" => true,
            "tenant_id" => true,
            "tid" => true,
            _ => false
        };
    }

    #endregion
}