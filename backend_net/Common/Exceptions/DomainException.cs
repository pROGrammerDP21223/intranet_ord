namespace backend_net.Common.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// Follows exception hierarchy pattern
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception for entity not found scenarios
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityName, object? entityId = null)
        : base($"{entityName} not found{(entityId != null ? $" with ID: {entityId}" : "")}", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message, errorCode)
    {
    }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Validation failed", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base("Validation failed", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

