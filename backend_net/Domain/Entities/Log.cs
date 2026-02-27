using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_net.Domain.Entities;

public class Log : BaseEntity
{
    [MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty; // GET, POST, PUT, DELETE, etc.

    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty; // Full path of the API endpoint

    [MaxLength(50)]
    public string? Controller { get; set; } // Controller name

    [MaxLength(50)]
    public string? Action { get; set; } // Action method name

    public int? UserId { get; set; } // User who made the request (nullable for unauthenticated requests)

    [MaxLength(255)]
    public string? UserName { get; set; } // User name for quick reference

    [MaxLength(255)]
    public string? UserEmail { get; set; } // User email for quick reference

    [MaxLength(50)]
    public string? UserRole { get; set; } // User role for quick reference

    public int StatusCode { get; set; } // HTTP status code

    [MaxLength(45)]
    public string? IpAddress { get; set; } // Client IP address

    [MaxLength(500)]
    public string? UserAgent { get; set; } // Browser/client user agent

    [Column(TypeName = "TEXT")]
    public string? RequestBody { get; set; } // Request body (for POST/PUT requests, truncated if too long)

    [Column(TypeName = "TEXT")]
    public string? ResponseBody { get; set; } // Response body (truncated if too long)

    public long? ResponseTimeMs { get; set; } // Response time in milliseconds

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; } // Error message if any (though we only log successful requests)

    [MaxLength(500)]
    public string? QueryString { get; set; } // Query string parameters
}

