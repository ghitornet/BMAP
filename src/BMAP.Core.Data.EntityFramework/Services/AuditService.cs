using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Abstractions;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Services;

/// <summary>
/// Service responsible for automatically setting audit fields on entities.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Sets audit fields for entity creation.
    /// </summary>
    /// <param name="entity">The entity being created.</param>
    void SetAuditFieldsForCreate(object entity);

    /// <summary>
    /// Sets audit fields for entity modification.
    /// </summary>
    /// <param name="entity">The entity being modified.</param>
    void SetAuditFieldsForUpdate(object entity);

    /// <summary>
    /// Sets audit fields for entity soft deletion.
    /// </summary>
    /// <param name="entity">The entity being soft deleted.</param>
    void SetAuditFieldsForSoftDelete(object entity);

    /// <summary>
    /// Gets the current user identifier for audit purposes.
    /// </summary>
    /// <returns>The current user identifier or a default value.</returns>
    string GetCurrentUserId();

    /// <summary>
    /// Gets the current user name for audit purposes.
    /// </summary>
    /// <returns>The current user name or a default value.</returns>
    string GetCurrentUserName();
}

/// <summary>
/// Default implementation of IAuditService that uses IUserContext.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUserContext _userContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IUserContext userContext, ILogger<AuditService> logger)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetAuditFieldsForCreate(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        _logger.LogTrace("Setting audit fields for create on entity {EntityType} by user {UserId}", 
            entity.GetType().Name, userId);

        // Set creation audit fields
        if (entity is ICreatable creatable)
        {
            creatable.CreatedAt = now;
            creatable.CreatedBy = userId;
        }

        // Set modification audit fields (same as creation for new entities)
        if (entity is IModifiable modifiable)
        {
            modifiable.LastModifiedAt = now;
            modifiable.LastModifiedBy = userId;
        }

        // Initialize soft delete fields
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = false;
            softDeletable.DeletedAt = null;
            softDeletable.DeletedBy = null;
        }

        // Set tenant information if supported
        if (entity is ITenantAware tenantAware && !string.IsNullOrEmpty(_userContext.TenantId))
        {
            tenantAware.TenantId = _userContext.TenantId;
        }
    }

    public void SetAuditFieldsForUpdate(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        _logger.LogTrace("Setting audit fields for update on entity {EntityType} by user {UserId}", 
            entity.GetType().Name, userId);

        // Set modification audit fields
        if (entity is IModifiable modifiable)
        {
            modifiable.LastModifiedAt = now;
            modifiable.LastModifiedBy = userId;
        }
    }

    public void SetAuditFieldsForSoftDelete(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        _logger.LogTrace("Setting audit fields for soft delete on entity {EntityType} by user {UserId}", 
            entity.GetType().Name, userId);

        // Set soft delete fields
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = now;
            softDeletable.DeletedBy = userId;
        }

        // Also update modification fields
        if (entity is IModifiable modifiable)
        {
            modifiable.LastModifiedAt = now;
            modifiable.LastModifiedBy = userId;
        }
    }

    public string GetCurrentUserId()
    {
        var userId = _userContext.UserId;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("No user ID available in context, using SYSTEM as fallback");
            return "SYSTEM";
        }

        return userId;
    }

    public string GetCurrentUserName()
    {
        var userName = _userContext.UserName;
        
        if (string.IsNullOrEmpty(userName))
        {
            _logger.LogWarning("No user name available in context, using System as fallback");
            return "System";
        }

        return userName;
    }
}

/// <summary>
/// Interface for entities that support tenant isolation.
/// </summary>
public interface ITenantAware
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    string? TenantId { get; set; }
}