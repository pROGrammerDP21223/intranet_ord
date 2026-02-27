using backend_net.Controllers;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using backend_net.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnquiriesController : BaseController
{
    private readonly IEnquiryService _enquiryService;
    private readonly ISecurityService _securityService;
    private readonly IAccessControlService _accessControlService;

    public EnquiriesController(IEnquiryService enquiryService, ISecurityService securityService, IAccessControlService accessControlService)
    {
        _enquiryService = enquiryService;
        _securityService = securityService;
        _accessControlService = accessControlService;
    }

    /// <summary>
    /// Create a new enquiry - Requires API Key authentication
    /// </summary>
    [HttpPost]
    [RequireApiKey]
    public async Task<IActionResult> CreateEnquiry([FromBody] CreateEnquiryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get ClientId from API key (set by RequireApiKey attribute)
            var clientIdFromApiKey = HttpContext.Items["ClientId"] as int?;
            if (clientIdFromApiKey.HasValue)
            {
                // Override ClientId from request with the one from API key for security
                request.ClientId = clientIdFromApiKey.Value;
            }

            // Security validation for website source
            if (request.Source?.ToLower() == "website")
            {
                // Validate CAPTCHA
                if (string.IsNullOrWhiteSpace(request.CaptchaToken))
                {
                    return HandleError("CAPTCHA token is required for website enquiries", 400);
                }

                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var captchaValid = await _securityService.ValidateCaptchaAsync(request.CaptchaToken, remoteIp);
                if (!captchaValid)
                {
                    return HandleError("CAPTCHA validation failed", 400);
                }

                // Validate CSRF token
                if (string.IsNullOrWhiteSpace(request.CsrfToken))
                {
                    return HandleError("CSRF token is required for website enquiries", 400);
                }

                var csrfValid = _securityService.ValidateCsrfToken(request.CsrfToken);
                if (!csrfValid)
                {
                    return HandleError("CSRF token validation failed", 400);
                }

                // Validate Honeypot (should be empty)
                var honeypotValid = _securityService.ValidateHoneypot(request.Honeypot);
                if (!honeypotValid)
                {
                    // Silently fail for bots (don't reveal honeypot detection)
                    return HandleSuccess("Enquiry submitted successfully", null);
                }
            }

            var enquiry = await _enquiryService.CreateAsync(request);
            
            var enquiryDto = new
            {
                enquiry.Id,
                enquiry.FullName,
                enquiry.MobileNumber,
                enquiry.EmailId,
                enquiry.ClientId,
                enquiry.Status,
                enquiry.Source,
                enquiry.ReferrerUrl,
                enquiry.CreatedAt
            };

            return StatusCode(201, new { message = "Enquiry submitted successfully", data = enquiryDto, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get all enquiries - PRIVATE ENDPOINT (requires authentication)
    /// Role-based access: Client (own), Sales Person (their clients), Sales Manager (own + sales persons' clients), Owner/Calling Staff (all)
    /// Query parameters: startDate (optional), endDate (optional)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllEnquiries([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            IEnumerable<Enquiry> enquiries;

            // Owner and Calling Staff can view all enquiries
            if (IsOwner() || IsCallingStaff())
            {
                enquiries = await _enquiryService.GetAllAsync(startDate, endDate);
            }
            else
            {
                // Get accessible client IDs based on role
                var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
                
                if (!accessibleClientIds.Any())
                {
                    return HandleSuccess("Enquiries retrieved successfully", new List<object>());
                }

                enquiries = await _enquiryService.GetFilteredByClientIdsAsync(accessibleClientIds, startDate, endDate);
            }

            var enquiriesList = enquiries?.ToList() ?? new List<Enquiry>();
            var enquiriesDto = enquiriesList.Select(e => new
            {
                e.Id,
                e.FullName,
                e.MobileNumber,
                e.EmailId,
                e.ClientId,
                ClientName = e.Client?.CompanyName,
                e.Status,
                e.Notes,
                e.Source,
                e.ReferrerUrl,
                e.RawPayload,
                e.ResolvedAt,
                e.CreatedAt,
                e.UpdatedAt
            }).ToList();

            return HandleSuccess("Enquiries retrieved successfully", enquiriesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get enquiry by ID - PRIVATE ENDPOINT (requires authentication)
    /// Role-based access: Client (own), Sales Person (their clients), Sales Manager (own + sales persons' clients), Owner/Calling Staff (all)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetEnquiryById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var enquiry = await _enquiryService.GetByIdAsync(id);
            if (enquiry == null)
            {
                return HandleError("Enquiry not found", 404);
            }

            // Check if user can access this enquiry's client
            if (!IsOwner() && !IsCallingStaff())
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, enquiry.ClientId))
                {
                    return HandleError("Unauthorized: You don't have access to this enquiry", 403);
                }
            }

            var enquiryDto = new
            {
                enquiry.Id,
                enquiry.FullName,
                enquiry.MobileNumber,
                enquiry.EmailId,
                enquiry.ClientId,
                ClientName = enquiry.Client?.CompanyName,
                enquiry.Status,
                enquiry.Notes,
                enquiry.Source,
                enquiry.ReferrerUrl,
                enquiry.RawPayload,
                enquiry.ResolvedAt,
                enquiry.CreatedAt,
                enquiry.UpdatedAt
            };

            return HandleSuccess("Enquiry retrieved successfully", enquiryDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get enquiries by status - PRIVATE ENDPOINT (requires authentication)
    /// Role-based access: Client (own), Sales Person (their clients), Sales Manager (own + sales persons' clients), Owner/Calling Staff (all)
    /// Query parameters: startDate (optional), endDate (optional)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize]
    public async Task<IActionResult> GetEnquiriesByStatus(string status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            IEnumerable<Enquiry> enquiries;

            // Owner and Calling Staff can view all enquiries
            if (IsOwner() || IsCallingStaff())
            {
                enquiries = await _enquiryService.GetByStatusAsync(status, startDate, endDate);
            }
            else
            {
                // Get accessible client IDs based on role
                var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
                
                if (!accessibleClientIds.Any())
                {
                    return HandleSuccess("Enquiries retrieved successfully", new List<object>());
                }

                enquiries = await _enquiryService.GetByStatusFilteredByClientIdsAsync(status, accessibleClientIds, startDate, endDate);
            }

            var enquiriesDto = enquiries.Select(e => new
            {
                e.Id,
                e.FullName,
                e.MobileNumber,
                e.EmailId,
                e.ClientId,
                ClientName = e.Client?.CompanyName,
                e.Status,
                e.Notes,
                e.Source,
                e.ReferrerUrl,
                e.RawPayload,
                e.ResolvedAt,
                e.CreatedAt,
                e.UpdatedAt
            }).ToList();

            return HandleSuccess("Enquiries retrieved successfully", enquiriesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get enquiries by client ID - PRIVATE ENDPOINT (requires authentication)
    /// Role-based access: Client (own), Sales Person (their clients), Sales Manager (own + sales persons' clients), Owner/Calling Staff (all)
    /// Query parameters: startDate (optional), endDate (optional)
    /// </summary>
    [HttpGet("client/{clientId}")]
    [Authorize]
    public async Task<IActionResult> GetEnquiriesByClient(int clientId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Check if user can access this client
            if (!IsOwner() && !IsCallingStaff())
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, clientId))
                {
                    return HandleError("Unauthorized: You don't have access to this client's enquiries", 403);
                }
            }

            var enquiries = await _enquiryService.GetByClientIdAsync(clientId, startDate, endDate);
            var enquiriesDto = enquiries.Select(e => new
            {
                e.Id,
                e.FullName,
                e.MobileNumber,
                e.EmailId,
                e.ClientId,
                ClientName = e.Client?.CompanyName,
                e.Status,
                e.Notes,
                e.Source,
                e.ReferrerUrl,
                e.RawPayload,
                e.ResolvedAt,
                e.CreatedAt,
                e.UpdatedAt
            }).ToList();

            return HandleSuccess("Enquiries retrieved successfully", enquiriesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Update enquiry - PRIVATE ENDPOINT (requires authentication)
    /// Role-based access: Only Owner/Calling Staff can update enquiries
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateEnquiry(int id, [FromBody] UpdateEnquiryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Owner and Calling Staff can update enquiries
            if (!IsOwner() && !IsCallingStaff())
            {
                return HandleError("Unauthorized: You don't have permission to update enquiries", 403);
            }

            // Check if user can access this enquiry's client
            var enquiry = await _enquiryService.GetByIdAsync(id);
            if (enquiry == null)
            {
                return HandleError("Enquiry not found", 404);
            }

            if (!IsOwner() && !IsCallingStaff())
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, enquiry.ClientId))
                {
                    return HandleError("Unauthorized: You don't have access to this enquiry", 403);
                }
            }

            var updatedEnquiry = await _enquiryService.UpdateAsync(id, request);
            
            var enquiryDto = new
            {
                updatedEnquiry.Id,
                updatedEnquiry.FullName,
                updatedEnquiry.MobileNumber,
                updatedEnquiry.EmailId,
                updatedEnquiry.ClientId,
                ClientName = updatedEnquiry.Client?.CompanyName,
                updatedEnquiry.Status,
                updatedEnquiry.Notes,
                updatedEnquiry.Source,
                updatedEnquiry.ReferrerUrl,
                updatedEnquiry.RawPayload,
                updatedEnquiry.ResolvedAt,
                updatedEnquiry.UpdatedAt
            };

            return HandleSuccess("Enquiry updated successfully", enquiryDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete enquiry - PRIVATE ENDPOINT (requires authentication)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEnquiry(int id)
    {
        try
        {
            // Only Admin/Owner can delete enquiries
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete enquiries", 403);
            }

            var result = await _enquiryService.DeleteAsync(id);
            if (!result)
            {
                return HandleError("Enquiry not found", 404);
            }

            return HandleSuccess("Enquiry deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get enquiry statistics - PRIVATE ENDPOINT (requires authentication)
    /// Role-based access: Client (own), Sales Person (their clients), Sales Manager (own + sales persons' clients), Owner/Calling Staff (all)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            int total, newCount, inProgressCount, resolvedCount, closedCount;

            // Owner and Calling Staff can view all statistics
            if (IsOwner() || IsCallingStaff())
            {
                total = await _enquiryService.GetTotalCountAsync();
                newCount = await _enquiryService.GetCountByStatusAsync("New");
                inProgressCount = await _enquiryService.GetCountByStatusAsync("In Progress");
                resolvedCount = await _enquiryService.GetCountByStatusAsync("Resolved");
                closedCount = await _enquiryService.GetCountByStatusAsync("Closed");
            }
            else
            {
                // Get accessible client IDs based on role
                var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
                
                if (!accessibleClientIds.Any())
                {
                    return HandleSuccess("Statistics retrieved successfully", new
                    {
                        Total = 0,
                        New = 0,
                        InProgress = 0,
                        Resolved = 0,
                        Closed = 0
                    });
                }

                total = await _enquiryService.GetTotalCountFilteredByClientIdsAsync(accessibleClientIds);
                newCount = await _enquiryService.GetCountByStatusFilteredByClientIdsAsync("New", accessibleClientIds);
                inProgressCount = await _enquiryService.GetCountByStatusFilteredByClientIdsAsync("In Progress", accessibleClientIds);
                resolvedCount = await _enquiryService.GetCountByStatusFilteredByClientIdsAsync("Resolved", accessibleClientIds);
                closedCount = await _enquiryService.GetCountByStatusFilteredByClientIdsAsync("Closed", accessibleClientIds);
            }

            var statistics = new
            {
                Total = total,
                New = newCount,
                InProgress = inProgressCount,
                Resolved = resolvedCount,
                Closed = closedCount
            };

            return HandleSuccess("Statistics retrieved successfully", statistics);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

